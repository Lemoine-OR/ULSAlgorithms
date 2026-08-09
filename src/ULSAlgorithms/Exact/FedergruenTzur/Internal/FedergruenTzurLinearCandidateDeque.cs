using System.Buffers;

namespace ULSAlgorithms.Exact.FedergruenTzur.Internal;

/// <summary>
/// Array-backed monotone candidate deque used by the two linear-time
/// Federgruen-Tzur specializations.
/// </summary>
/// <remarks>
/// Candidate slopes are stored in strictly decreasing order. Envelope
/// activation thresholds are therefore maintained by deleting only from the
/// front or the back, exactly as in Sections 3 and 4 of Federgruen and Tzur
/// (1991).
/// </remarks>
internal sealed class FedergruenTzurLinearCandidateDeque : IDisposable
{
    private readonly double[] _slope;
    private readonly double[] _intercept;
    private readonly double[] _startX;
    private readonly int[] _period;

    private int _head;
    private int _tail = -1;
    private bool _disposed;

    public FedergruenTzurLinearCandidateDeque(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                capacity,
                "Candidate-deque capacity must be positive.");
        }

        _slope = ArrayPool<double>.Shared.Rent(capacity);
        _intercept = ArrayPool<double>.Shared.Rent(capacity);
        _startX = ArrayPool<double>.Shared.Rent(capacity);
        _period = ArrayPool<int>.Shared.Rent(capacity);
    }

    public bool IsEmpty
    {
        get
        {
            ThrowIfDisposed();
            return _tail < _head;
        }
    }

    public double LastSlope
    {
        get
        {
            ThrowIfDisposed();

            if (IsEmpty)
            {
                throw new InvalidOperationException(
                    "The candidate deque is empty.");
            }

            return _slope[_tail];
        }
    }

    /// <summary>
    /// Adds a candidate whose slope must not exceed the slope of the current
    /// last candidate. Equal-slope candidates are reduced to the globally
    /// dominating intercept.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the candidate remains on the lower envelope.
    /// </returns>
    public bool AddMonotone(
        int period,
        double slope,
        double intercept)
    {
        ThrowIfDisposed();

        if (!double.IsFinite(slope) ||
            !double.IsFinite(intercept))
        {
            throw new ArithmeticException(
                "Federgruen-Tzur candidate coefficients must be finite.");
        }

        while (!IsEmpty)
        {
            var lastSlope = _slope[_tail];

            if (slope > lastSlope)
            {
                throw new InvalidOperationException(
                    "A linear Federgruen-Tzur specialization received " +
                    "nonmonotone candidate slopes.");
            }

            if (slope < lastSlope)
            {
                break;
            }

            if (_intercept[_tail] <= intercept)
            {
                return false;
            }

            _tail--;
        }

        var activationX = double.NegativeInfinity;

        while (!IsEmpty)
        {
            activationX = IntersectionX(
                _slope[_tail],
                _intercept[_tail],
                slope,
                intercept);

            if (_tail == _head ||
                activationX > _startX[_tail])
            {
                break;
            }

            _tail--;
        }

        if (IsEmpty)
        {
            activationX = double.NegativeInfinity;
        }
        else
        {
            activationX = IntersectionX(
                _slope[_tail],
                _intercept[_tail],
                slope,
                intercept);
        }

        _tail++;

        _period[_tail] = period;
        _slope[_tail] = slope;
        _intercept[_tail] = intercept;
        _startX[_tail] = activationX;

        return true;
    }

    /// <summary>
    /// Returns the active candidate at the supplied cumulative demand and
    /// permanently discards candidate intervals that lie strictly in the past.
    /// </summary>
    public int GetBestAndDiscardPast(double cumulativeDemand)
    {
        ThrowIfDisposed();

        if (!double.IsFinite(cumulativeDemand))
        {
            throw new ArithmeticException(
                "Cumulative demand must be finite.");
        }

        if (IsEmpty)
        {
            throw new InvalidOperationException(
                "The candidate deque is empty.");
        }

        while (_head < _tail &&
               _startX[_head + 1] < cumulativeDemand)
        {
            _head++;
        }

        return _period[_head];
    }

    public double BestSlope
    {
        get
        {
            ThrowIfDisposed();

            if (IsEmpty)
            {
                throw new InvalidOperationException(
                    "The candidate deque is empty.");
            }

            return _slope[_head];
        }
    }

    public double BestIntercept
    {
        get
        {
            ThrowIfDisposed();

            if (IsEmpty)
            {
                throw new InvalidOperationException(
                    "The candidate deque is empty.");
            }

            return _intercept[_head];
        }
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
        ArrayPool<int>.Shared.Return(_period, clearArray: false);
    }

    private static double IntersectionX(
        double higherSlope,
        double higherIntercept,
        double lowerSlope,
        double lowerIntercept)
    {
        var denominator = higherSlope - lowerSlope;

        if (!(denominator > 0.0) ||
            !double.IsFinite(denominator))
        {
            throw new ArithmeticException(
                "A linear Federgruen-Tzur envelope intersection has " +
                "an invalid denominator.");
        }

        var numerator = lowerIntercept - higherIntercept;
        var intersection = numerator / denominator;

        if (!double.IsFinite(intersection))
        {
            throw new ArithmeticException(
                "A linear Federgruen-Tzur envelope intersection is not finite.");
        }

        return intersection;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
