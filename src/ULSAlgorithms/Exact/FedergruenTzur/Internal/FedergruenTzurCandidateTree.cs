using System.Buffers;

namespace ULSAlgorithms.Exact.FedergruenTzur.Internal;

/// <summary>
/// Array-backed balanced candidate tree for the Federgruen-Tzur forward algorithm.
/// </summary>
/// <remarks>
/// <para>
/// Candidate periods are ordered by their transformed variable-cost coefficient.
/// The doubly linked order represents the Minimal Optimal Predecessor envelope,
/// while the AVL links provide logarithmic insertion and deletion without
/// allocating one managed object per candidate.
/// </para>
/// <para>
/// For two adjacent candidates, <c>StartX</c> is the cumulative-demand threshold
/// at which the lower-slope candidate becomes at least as attractive as its
/// predecessor. These thresholds are the geometric counterpart of the
/// <c>G(k,l)</c> values in Federgruen and Tzur (1991).
/// </para>
/// </remarks>
internal sealed class FedergruenTzurCandidateTree : IDisposable
{
    private readonly int _capacity;

    private readonly double[] _slope;
    private readonly double[] _intercept;
    private readonly double[] _startX;

    private readonly int[] _left;
    private readonly int[] _right;
    private readonly int[] _parent;
    private readonly int[] _height;
    private readonly int[] _previous;
    private readonly int[] _next;

    private int _root = -1;
    private int _first = -1;
    private bool _disposed;

    public FedergruenTzurCandidateTree(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                capacity,
                "Candidate-tree capacity must be positive.");
        }

        _capacity = capacity;

        _slope = ArrayPool<double>.Shared.Rent(capacity);
        _intercept = ArrayPool<double>.Shared.Rent(capacity);
        _startX = ArrayPool<double>.Shared.Rent(capacity);

        _left = ArrayPool<int>.Shared.Rent(capacity);
        _right = ArrayPool<int>.Shared.Rent(capacity);
        _parent = ArrayPool<int>.Shared.Rent(capacity);
        _height = ArrayPool<int>.Shared.Rent(capacity);
        _previous = ArrayPool<int>.Shared.Rent(capacity);
        _next = ArrayPool<int>.Shared.Rent(capacity);
    }

    public double GetSlope(int period)
    {
        ValidateNode(period);
        return _slope[period];
    }

    public double GetIntercept(int period)
    {
        ValidateNode(period);
        return _intercept[period];
    }

    /// <summary>
    /// Inserts one candidate line and removes candidates that can never be
    /// optimal for any future cumulative demand.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the candidate remains in the minimal envelope;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Add(
        int period,
        double slope,
        double intercept)
    {
        ThrowIfDisposed();

        if ((uint)period >= (uint)_capacity)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        if (!double.IsFinite(slope) || !double.IsFinite(intercept))
        {
            throw new ArithmeticException(
                "Federgruen-Tzur candidate coefficients must be finite.");
        }

        var equalSlope = FindBySlope(slope);

        if (equalSlope >= 0)
        {
            // A lower intercept dominates globally. With equal intercepts the
            // earlier period is retained, which is Federgruen-Tzur's canonical
            // lowest-index tie breaking because periods arrive chronologically.
            if (_intercept[equalSlope] <= intercept)
            {
                return false;
            }

            Remove(equalSlope);
        }

        InitializeNode(period, slope, intercept);

        var higherSlope = -1;
        var lowerSlope = -1;
        var treeParent = -1;
        var current = _root;

        while (current >= 0)
        {
            treeParent = current;

            if (slope < _slope[current])
            {
                higherSlope = current;
                current = _left[current];
            }
            else
            {
                lowerSlope = current;
                current = _right[current];
            }
        }

        _parent[period] = treeParent;

        if (treeParent < 0)
        {
            _root = period;
        }
        else if (slope < _slope[treeParent])
        {
            _left[treeParent] = period;
        }
        else
        {
            _right[treeParent] = period;
        }

        // Hull order is nonincreasing in slope:
        // Previous = higher slope, Next = lower slope.
        _previous[period] = higherSlope;
        _next[period] = lowerSlope;

        if (higherSlope >= 0)
        {
            _next[higherSlope] = period;
        }
        else
        {
            _first = period;
        }

        if (lowerSlope >= 0)
        {
            _previous[lowerSlope] = period;
        }

        Rebalance(treeParent);

        if (higherSlope >= 0 &&
            lowerSlope >= 0 &&
            IntersectionX(higherSlope, period) >=
            IntersectionX(period, lowerSlope))
        {
            Remove(period);
            return false;
        }

        var previous = _previous[period];

        while (previous >= 0)
        {
            var previousPrevious = _previous[previous];

            if (previousPrevious < 0 ||
                IntersectionX(previousPrevious, previous) <
                IntersectionX(previous, period))
            {
                break;
            }

            Remove(previous);
            previous = _previous[period];
        }

        var next = _next[period];

        while (next >= 0)
        {
            var nextNext = _next[next];

            if (nextNext < 0 ||
                IntersectionX(period, next) <
                IntersectionX(next, nextNext))
            {
                break;
            }

            Remove(next);
            next = _next[period];
        }

        previous = _previous[period];
        next = _next[period];

        _startX[period] =
            previous < 0
                ? double.NegativeInfinity
                : IntersectionX(previous, period);

        if (next >= 0)
        {
            _startX[next] = IntersectionX(period, next);
        }

        return true;
    }

    /// <summary>
    /// Removes envelope candidates whose optimality interval lies entirely
    /// before the current cumulative demand and returns the current best period.
    /// </summary>
    public int GetBestAndDiscardPast(double cumulativeDemand)
    {
        ThrowIfDisposed();

        if (!double.IsFinite(cumulativeDemand))
        {
            throw new ArithmeticException(
                "Cumulative demand must be finite.");
        }

        if (_first < 0)
        {
            throw new InvalidOperationException(
                "The Federgruen-Tzur candidate tree is empty.");
        }

        while (_next[_first] >= 0)
        {
            var current = _first;
            var next = _next[current];
            var threshold = _startX[next];

            var nextIsPreferred =
                threshold < cumulativeDemand ||
                (threshold == cumulativeDemand && next < current);

            if (!nextIsPreferred)
            {
                break;
            }

            Remove(current);
        }

        return _first;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        ArrayPool<double>.Shared.Return(_slope, clearArray: false);
        ArrayPool<double>.Shared.Return(_intercept, clearArray: false);
        ArrayPool<double>.Shared.Return(_startX, clearArray: false);

        ArrayPool<int>.Shared.Return(_left, clearArray: false);
        ArrayPool<int>.Shared.Return(_right, clearArray: false);
        ArrayPool<int>.Shared.Return(_parent, clearArray: false);
        ArrayPool<int>.Shared.Return(_height, clearArray: false);
        ArrayPool<int>.Shared.Return(_previous, clearArray: false);
        ArrayPool<int>.Shared.Return(_next, clearArray: false);
    }

    private void InitializeNode(
        int period,
        double slope,
        double intercept)
    {
        _slope[period] = slope;
        _intercept[period] = intercept;
        _startX[period] = double.NegativeInfinity;

        _left[period] = -1;
        _right[period] = -1;
        _parent[period] = -1;
        _height[period] = 1;
        _previous[period] = -1;
        _next[period] = -1;
    }

    private int FindBySlope(double slope)
    {
        var current = _root;

        while (current >= 0)
        {
            if (slope < _slope[current])
            {
                current = _left[current];
            }
            else if (slope > _slope[current])
            {
                current = _right[current];
            }
            else
            {
                return current;
            }
        }

        return -1;
    }

    private double IntersectionX(
        int higherSlopePeriod,
        int lowerSlopePeriod)
    {
        var denominator =
            _slope[higherSlopePeriod] -
            _slope[lowerSlopePeriod];

        if (!(denominator > 0.0) ||
            !double.IsFinite(denominator))
        {
            throw new ArithmeticException(
                "A Federgruen-Tzur candidate intersection has an invalid denominator.");
        }

        var numerator =
            _intercept[lowerSlopePeriod] -
            _intercept[higherSlopePeriod];

        var intersection = numerator / denominator;

        if (!double.IsFinite(intersection))
        {
            throw new ArithmeticException(
                "A Federgruen-Tzur candidate intersection is not finite.");
        }

        return intersection;
    }

    private void Remove(int node)
    {
        var previous = _previous[node];
        var next = _next[node];

        if (previous >= 0)
        {
            _next[previous] = next;
        }
        else
        {
            _first = next;
        }

        if (next >= 0)
        {
            _previous[next] = previous;

            if (previous < 0)
            {
                _startX[next] = double.NegativeInfinity;
            }
        }

        DeleteTreeNode(node);

        _previous[node] = -1;
        _next[node] = -1;
    }

    private void DeleteTreeNode(int node)
    {
        int rebalanceStart;

        if (_left[node] < 0)
        {
            rebalanceStart = _parent[node];
            Transplant(node, _right[node]);
        }
        else if (_right[node] < 0)
        {
            rebalanceStart = _parent[node];
            Transplant(node, _left[node]);
        }
        else
        {
            var successor = Minimum(_right[node]);

            if (_parent[successor] == node)
            {
                Transplant(node, successor);

                _left[successor] = _left[node];
                _parent[_left[successor]] = successor;

                UpdateHeight(successor);
                rebalanceStart = successor;
            }
            else
            {
                var successorOldParent = _parent[successor];

                Transplant(successor, _right[successor]);

                _right[successor] = _right[node];
                _parent[_right[successor]] = successor;

                Transplant(node, successor);

                _left[successor] = _left[node];
                _parent[_left[successor]] = successor;

                UpdateHeight(successor);
                rebalanceStart = successorOldParent;
            }
        }

        _left[node] = -1;
        _right[node] = -1;
        _parent[node] = -1;
        _height[node] = 0;

        Rebalance(rebalanceStart);
    }

    private int Minimum(int node)
    {
        var current = node;

        while (_left[current] >= 0)
        {
            current = _left[current];
        }

        return current;
    }

    private void Transplant(int oldNode, int replacement)
    {
        var oldParent = _parent[oldNode];

        if (oldParent < 0)
        {
            _root = replacement;
        }
        else if (_left[oldParent] == oldNode)
        {
            _left[oldParent] = replacement;
        }
        else
        {
            _right[oldParent] = replacement;
        }

        if (replacement >= 0)
        {
            _parent[replacement] = oldParent;
        }
    }

    private void Rebalance(int node)
    {
        var current = node;

        while (current >= 0)
        {
            UpdateHeight(current);

            var balance = Balance(current);
            var subtreeRoot = current;

            if (balance > 1)
            {
                var left = _left[current];

                if (Balance(left) < 0)
                {
                    RotateLeft(left);
                }

                subtreeRoot = RotateRight(current);
            }
            else if (balance < -1)
            {
                var right = _right[current];

                if (Balance(right) > 0)
                {
                    RotateRight(right);
                }

                subtreeRoot = RotateLeft(current);
            }

            current = _parent[subtreeRoot];
        }
    }

    private int RotateLeft(int node)
    {
        var pivot = _right[node];

        if (pivot < 0)
        {
            throw new InvalidOperationException(
                "Invalid AVL left rotation.");
        }

        var middle = _left[pivot];
        var oldParent = _parent[node];

        _parent[pivot] = oldParent;

        if (oldParent < 0)
        {
            _root = pivot;
        }
        else if (_left[oldParent] == node)
        {
            _left[oldParent] = pivot;
        }
        else
        {
            _right[oldParent] = pivot;
        }

        _left[pivot] = node;
        _parent[node] = pivot;

        _right[node] = middle;

        if (middle >= 0)
        {
            _parent[middle] = node;
        }

        UpdateHeight(node);
        UpdateHeight(pivot);

        return pivot;
    }

    private int RotateRight(int node)
    {
        var pivot = _left[node];

        if (pivot < 0)
        {
            throw new InvalidOperationException(
                "Invalid AVL right rotation.");
        }

        var middle = _right[pivot];
        var oldParent = _parent[node];

        _parent[pivot] = oldParent;

        if (oldParent < 0)
        {
            _root = pivot;
        }
        else if (_left[oldParent] == node)
        {
            _left[oldParent] = pivot;
        }
        else
        {
            _right[oldParent] = pivot;
        }

        _right[pivot] = node;
        _parent[node] = pivot;

        _left[node] = middle;

        if (middle >= 0)
        {
            _parent[middle] = node;
        }

        UpdateHeight(node);
        UpdateHeight(pivot);

        return pivot;
    }

    private int Balance(int node)
    {
        if (node < 0)
        {
            return 0;
        }

        return Height(_left[node]) - Height(_right[node]);
    }

    private int Height(int node)
    {
        return node < 0 ? 0 : _height[node];
    }

    private void UpdateHeight(int node)
    {
        if (node < 0)
        {
            return;
        }

        _height[node] =
            1 + Math.Max(
                Height(_left[node]),
                Height(_right[node]));
    }

    private void ValidateNode(int period)
    {
        ThrowIfDisposed();

        if ((uint)period >= (uint)_capacity ||
            _height[period] <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(period),
                period,
                "The candidate period is not active in the tree.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
