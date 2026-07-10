const BASE_URL = "/tickets";

async function handleResponse(response) {
  if (!response.ok) {
    let message = `Request failed (HTTP ${response.status})`;
    try {
      const problem = await response.json();
      message = problem.title || problem.detail || JSON.stringify(problem);
    } catch {}
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

async function addComment(ticketId, authorRole, message) {
  return handleResponse(
    await fetch(`${BASE_URL}/${ticketId}/comments`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ authorRole, message }),
    })
  );
}

function buildCommentsHtml(ticket) {
  const comments = ticket.comments
    .map(
      (c) => `
        <li>
          <strong>${c.authorRole}</strong>
          <small>${new Date(c.createdAt).toLocaleString()}</small>
          <p>${c.message}</p>
        </li>`
    )
    .join("");

  return `
    <div class="comments">
      <h4>Échanges</h4>
      <ul class="comment-list">${comments || "<li>Aucun message.</li>"}</ul>
      <form class="comment-form">
        <textarea name="message" maxlength="1000" required placeholder="Ajouter un message..."></textarea>
        <button type="submit">Envoyer</button>
      </form>
    </div>`;
}

function wireCommentForm(card, ticketId, authorRole, onDone, onError) {
  card.querySelector(".comment-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    const message = event.target.message.value;
    try {
      await addComment(ticketId, authorRole, message);
      await onDone();
    } catch (err) {
      onError(`Impossible d'ajouter le commentaire au ticket #${ticketId} : ${err.message}`);
    }
  });
}
