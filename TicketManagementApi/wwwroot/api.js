const BASE_URL = "/tickets";

// fetch() only rejects on a network failure -- a 400/404 response still
// resolves successfully, so every call must check response.ok before
// trusting the body.
async function handleResponse(response) {
  if (!response.ok) {
    let message = `Request failed (HTTP ${response.status})`;
    try {
      const problem = await response.json();
      message = problem.title || problem.detail || JSON.stringify(problem);
    } catch {
      // no JSON body -- keep the generic message above
    }
    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

async function getTickets() {
  return handleResponse(await fetch(BASE_URL));
}

async function createTicket(ticket) {
  return handleResponse(
    await fetch(BASE_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(ticket),
    })
  );
}

async function updateStatus(id, status) {
  return handleResponse(
    await fetch(`${BASE_URL}/${id}/status`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ status }),
    })
  );
}

async function deleteTicket(id) {
  return handleResponse(await fetch(`${BASE_URL}/${id}`, { method: "DELETE" }));
}
