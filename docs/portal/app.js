(() => {
  const input = document.getElementById("algorithmSearch");
  const cards = [...document.querySelectorAll(".algorithm-card")];
  const buttons = [...document.querySelectorAll(".filter")];
  const sections = [...document.querySelectorAll(".algorithm-section")];
  const count = document.getElementById("resultCount");
  let group = "all";

  function apply() {
    const query = (input?.value || "").trim().toLowerCase();
    let visible = 0;
    for (const card of cards) {
      const matchesGroup = group === "all" || card.dataset.group === group;
      const matchesText = !query || (card.dataset.search || "").includes(query);
      const show = matchesGroup && matchesText;
      card.classList.toggle("hidden-card", !show);
      if (show) visible++;
    }
    for (const section of sections) {
      const hasVisible = [...section.querySelectorAll(".algorithm-card")]
        .some(card => !card.classList.contains("hidden-card"));
      section.classList.toggle("hidden-section", !hasVisible);
    }
    if (count) count.textContent = `${visible} ${visible === 1 ? "algorithm" : "algorithms"}`;
  }

  input?.addEventListener("input", apply);
  for (const button of buttons) {
    button.addEventListener("click", () => {
      group = button.dataset.filter || "all";
      buttons.forEach(x => x.classList.toggle("active", x === button));
      apply();
    });
  }

  for (const jump of document.querySelectorAll("[data-jump-filter]")) {
    jump.addEventListener("click", () => {
      group = jump.dataset.jumpFilter || "all";
      buttons.forEach(x => x.classList.toggle("active", x.dataset.filter === group));
      apply();
      document.getElementById("algorithms")?.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  }

  for (const button of document.querySelectorAll(".copy-code")) {
    button.addEventListener("click", async () => {
      const target = document.getElementById(button.dataset.copyTarget || "");
      if (!target) return;
      try {
        await navigator.clipboard.writeText(target.textContent || "");
        const original = button.textContent;
        button.textContent = "Copied";
        setTimeout(() => { button.textContent = original; }, 1200);
      } catch { /* Clipboard is optional; code remains selectable. */ }
    });
  }
})();
