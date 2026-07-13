const errorBox = document.getElementById("error-box");
const ticketList = document.getElementById("ticket-list");

const STATUSES = ["Open", "Waiting", "Resolved"];
const RESOLVED_DELETION_DELAY_MS = 2 * 60 * 1000;

// setInterval handles for the resolved-ticket countdowns currently on screen.
// Cleared and rebuilt on every renderTickets() call, otherwise each manual
// "Refresh" click would leak another full set of ticking intervals forever.
let countdownIntervals = [];

function clearCountdowns() {
  countdownIntervals.forEach(clearInterval);
  countdownIntervals = [];
}

function showError(message) {
  errorBox.textContent = message;
  errorBox.hidden = false;
}

function clearError() {
  errorBox.hidden = true;
  errorBox.textContent = "";
}

function renderTickets(tickets) {
  clearCountdowns();
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
      <p class="countdown"></p>
      ${buildCommentsHtml(ticket)}
    `;

    card.querySelector("select").addEventListener("change", async (event) => {
      try {
        clearError();
        await updateStatus(ticket.id, event.target.value);
        await loadTickets();
      } catch (err) {
        showError(`Impossible de mettre à jour le ticket #${ticket.id} : ${err.message}`);
        await loadTickets();
      }
    });

    card.querySelector("button.danger").addEventListener("click", async () => {
      try {
        clearError();
        await deleteTicket(ticket.id);
        card.remove();
      } catch (err) {
        showError(`Impossible de supprimer le ticket #${ticket.id} : ${err.message}`);
      }
    });

    wireCommentForm(card, ticket.id, "ITAgent", loadTickets, showError);

    if (ticket.status === "Resolved" && ticket.updatedAt) {
      startCountdown(card.querySelector(".countdown"), ticket.updatedAt);
    }

    ticketList.appendChild(card);
  }
}

// Ticks a "sera supprimé dans mm:ss" label toward the moment the background
// cleanup sweep (server-side, every ~30s) is expected to delete this ticket.
// The exact deletion can lag a bit behind this countdown reaching zero --
// that's expected, not a bug (see plan notes).
function startCountdown(countdownEl, updatedAt) {
  const deadline = new Date(updatedAt).getTime() + RESOLVED_DELETION_DELAY_MS;

  const tick = () => {
    const remainingMs = deadline - Date.now();
    if (remainingMs <= 0) {
      countdownEl.textContent = "Suppression imminente…";
      clearInterval(intervalId);
      loadTickets();
      return;
    }
    const totalSeconds = Math.floor(remainingMs / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = String(totalSeconds % 60).padStart(2, "0");
    countdownEl.textContent = `Sera supprimé dans ${minutes}:${seconds}`;
  };

  tick();
  const intervalId = setInterval(tick, 1000);
  countdownIntervals.push(intervalId);
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
