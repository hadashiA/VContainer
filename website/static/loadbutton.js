const timerHandle = setInterval(() => {
  const anchors = document.querySelectorAll("a.github-button");
  if (anchors.length > 0) {
    renderStarCount(anchors[0]);
    clearTimeout(timerHandle);
  }
}, 100);

async function renderStarCount(anchor) {
  const res = await fetch('https://api.github.com/repos/hadashiA/VContainer');
  const json = await res.json();
  anchor.innerText = `★ ${json.stargazers_count}`;
}
