const errorBox = document.getElementById("error-box");
const ticketList = document.getElementById("ticket-list");

const STATUSES = ["Open", "Waiting", "Resolved"];

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
        <label>
          Statut :
          <select>
            ${STATUSES.map(
              (status) =>
                `<option value="${status}" ${status === ticket.status ? "selected" : ""}>${status}</option>`
            ).join("")}
          </select>
        </label>
        <span><strong>Priorité :</strong> ${ticket.priority}</span>
        <button class="danger" type="button">Supprimer</button>
      </div>
    `;

    card.querySelector("select").addEventListener("change", async (event) => {
      try {
        clearError();
        await updateStatus(ticket.id, event.target.value);
      } catch (err) {
        showError(`Impossible de mettre à jour le ticket #${ticket.id} : ${err.message}`);
        await loadTickets();
      }
    });

    card.querySelector("button").addEventListener("click", async () => {
      try {
        clearError();
        await deleteTicket(ticket.id);
        card.remove();
      } catch (err) {
        showError(`Impossible de supprimer le ticket #${ticket.id} : ${err.message}`);
      }
    });

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

document.getElementById("refresh-button").addEventListener("click", loadTickets);

loadTickets();
