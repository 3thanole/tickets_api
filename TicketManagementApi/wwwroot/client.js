const form = document.getElementById("ticket-form");
const errorBox = document.getElementById("error-box");
const ticketList = document.getElementById("ticket-list");

function showError(message) {
  errorBox.textContent = message;
  errorBox.hidden = false;
}

function clearError() {
  errorBox.hidden = true;
  errorBox.textContent = "";
}

function renderTickets(tickets) {
  ticketList.innerHTML = "";

  if (tickets.length === 0) {
    ticketList.textContent = "Aucun ticket pour le moment.";
    return;
  }

  for (const ticket of tickets) {
    const card = document.createElement("div");
    card.className = "ticket-card";
    card.innerHTML = `
      <h3>#${ticket.id} — ${ticket.title}</h3>
      <p>${ticket.description ?? ""}</p>
      <div class="meta">
        <span><strong>Statut :</strong> ${ticket.status}</span>
        <span><strong>Priorité :</strong> ${ticket.priority}</span>
      </div>
      <p><small>Créé le ${new Date(ticket.createdAt).toLocaleString()}</small></p>
      ${buildCommentsHtml(ticket)}
    `;
    wireCommentForm(card, ticket.id, "Client", loadTickets, showError);
    ticketList.appendChild(card);
  }
}

async function loadTickets() {
  try {
    clearError();
    const tickets = await getTickets();
    renderTickets(tickets);
  } catch (err) {
    showError("Impossible de charger les tickets : " + err.message);
  }
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  clearError();

  const newTicket = {
    title: form.title.value,
    description: form.description.value,
    priority: form.priority.value,
  };

  try {
    await createTicket(newTicket);
    form.reset();
    await loadTickets();
  } catch (err) {
    showError("Impossible de créer le ticket : " + err.message);
  }
});

document.getElementById("refresh-button").addEventListener("click", loadTickets);

loadTickets();
