import http from "node:http";

const HOST = "127.0.0.1";
const PORT = 4318;
const MAX_ITEMS = 200;

/** @type {Array<{receivedAt: string, payload: unknown}>} */
let requests = [];

function addCorsHeaders(res) {
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type,Authorization");
}

function sendJson(res, status, body) {
  addCorsHeaders(res);
  res.statusCode = status;
  res.setHeader("Content-Type", "application/json");
  res.end(JSON.stringify(body));
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    let data = "";
    req.setEncoding("utf8");
    req.on("data", (chunk) => {
      data += chunk;
    });
    req.on("end", () => resolve(data));
    req.on("error", reject);
  });
}

const server = http.createServer(async (req, res) => {
  const method = req.method ?? "GET";
  const url = req.url ?? "/";

  if (method === "OPTIONS") {
    addCorsHeaders(res);
    res.statusCode = 204;
    res.end();
    return;
  }

  if (method === "GET" && url === "/health") {
    sendJson(res, 200, { ok: true });
    return;
  }

  if (method === "POST" && url === "/v1/traces") {
    const rawBody = await readBody(req);
    let payload;
    try {
      payload = rawBody.length > 0 ? JSON.parse(rawBody) : {};
    } catch {
      payload = { parseError: true, rawBody };
    }

    requests.push({
      receivedAt: new Date().toISOString(),
      payload,
    });

    if (requests.length > MAX_ITEMS) {
      requests = requests.slice(requests.length - MAX_ITEMS);
    }

    sendJson(res, 200, {});
    return;
  }

  if (method === "POST" && url === "/_otel/reset") {
    requests = [];
    sendJson(res, 200, { ok: true });
    return;
  }

  if (method === "GET" && url === "/_otel/requests") {
    sendJson(res, 200, {
      count: requests.length,
      items: requests,
    });
    return;
  }

  sendJson(res, 404, { error: "Not found" });
});

server.listen(PORT, HOST, () => {
  // Keep output compact so Playwright logs stay readable.
  console.log(`[otel-collector] listening on http://${HOST}:${PORT}`);
});
