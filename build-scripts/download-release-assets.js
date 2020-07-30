const http = require("https");
const fs = require("fs");

const assetsDir = `${__dirname}/../tmp_asset_downloads`;

const tagName = process.argv[2];

if (!tagName) {
	console.error("Missing tag name argument");
	process.exit(1);
}

if (!/v\d+\.\d+\.\d+/.test(tagName)) {
	console.error("Invalid tag name");
	process.exit(1);
}

if (!fs.existsSync(assetsDir)) {
	fs.mkdirSync(assetsDir);
}

function fetchLatest(repo) {
	return new Promise((resolve) =>
		http.get(
			{
				host: "api.github.com",
				path: `/repos/HearthSim/${repo}/releases/latest`,
				headers: { "User-Agent": "request" },
			},
			(res) => {
				let json = "";
				res.on("data", (data) => (json += data));
				res.on("end", () => resolve(JSON.parse(json)));
			}
		)
	);
}

function downloadFile(repo, file) {
	console.log(`Downloading ${file}...`);

	function get(url, resolve) {
		http.get(url, (res) => {
			if (res.statusCode === 301 || res.statusCode === 302) {
				return get(res.headers.location, resolve);
			}
			resolve(res);
		});
	}

	return new Promise((resolve) => {
		get(
			{
				host: "github.com",
				path: `/HearthSim/${repo}/releases/download/${tagName}/${file}`,
				headers: { "User-Agent": "request" },
			},
			(res) => {
				res.on("end", resolve);
				res.pipe(fs.createWriteStream(`${assetsDir}/${file}`));
			}
		);
	});
}

async function run() {
	console.log(`Verifying ${tagName} matches latest releases...`);
	const sqRelease = await fetchLatest("HDT-Releases");
	if (sqRelease.tag_name !== tagName) {
		throw new Error(
			`Latest Squirrel release (${sqRelease.tag_name}) does not match ${tagName}`
		);
	}

	const pRelease = await fetchLatest("Hearthstone-Deck-Tracker");
	if (pRelease.tag_name !== tagName) {
		throw new Error(
			`Latest Portable release (${pRelease.tag_name}) does not match ${tagName}`
		);
	}

	const version = tagName.slice(1);
	await downloadFile("HDT-Releases", "RELEASES");
	await downloadFile("HDT-Releases", "HDT-Installer.exe");
	await downloadFile(
		"HDT-Releases",
		`HearthstoneDeckTracker-${version}-delta.nupkg`
	);
	await downloadFile(
		"HDT-Releases",
		`HearthstoneDeckTracker-${version}-full.nupkg`
	);
	await downloadFile(
		"Hearthstone-Deck-Tracker",
		`Hearthstone.Deck.Tracker-${tagName}.zip`
	);
	console.log("Done.");
}

run().catch((error) => {
	console.error(error);
	process.exitCode = 1;
});
