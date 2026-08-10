(() => {
  const input = document.getElementById("algorithmSearch");
  const rows = [...document.querySelectorAll("#algorithmRows tr")];
  const buttons = [...document.querySelectorAll(".filter")];
  const count = document.getElementById("resultCount");
  let kind = "all";

  function apply() {
    const query = (input?.value || "").trim().toLowerCase();
    let visible = 0;

    for (const row of rows) {
      const matchesKind = kind === "all" || row.dataset.kind === kind;
      const matchesText = !query || (row.dataset.search || "").includes(query);
      const show = matchesKind && matchesText;
      row.classList.toggle("hidden-row", !show);
      if (show) visible++;
    }

    if (count) count.textContent = `${visible} ${visible === 1 ? "strategy" : "strategies"}`;
  }

  input?.addEventListener("input", apply);

  for (const button of buttons) {
    button.addEventListener("click", () => {
      kind = button.dataset.filter || "all";
      buttons.forEach(x => x.classList.toggle("active", x === button));
      apply();
    });
  }
})();
