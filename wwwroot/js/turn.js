// turn.js
function startInactivityTimer(seconds, onExpire) {
    let remaining = seconds;
    const el = document.getElementById("inactivity-timer");
    if (!el) return null;
    el.innerText = remaining;
    const interval = setInterval(() => {
        remaining--;
        el.innerText = remaining;
        if (remaining <= 0) {
            clearInterval(interval);
            onExpire();
        }
    }, 1000);
    return interval;
}

function lanzarDado() {
    fetch('/Turn/LanzarDadoAjax', { method: 'POST' })
        .then(r => r.json())
        .then(data => {
            alert("Valor del dado: " + data.valor);
            window.location.href = '/Turn/SeleccionFicha?valor=' + data.valor;
        })
        .catch(err => alert('Error al lanzar dado: ' + err));
}

function moverFicha(indiceFicha, desde, hasta) {
    fetch('/Turn/MoverFichaAjax', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `indiceFicha=${indiceFicha}&desde=${desde}&hasta=${hasta}`
    }).then(r => r.json())
      .then(data => {
          if (data.ok) {
              window.location.href = '/Turn/FinTurno';
          }
      });
}

function pasarTurno() {
    fetch('/Turn/PasarTurnoAjax', { method: 'POST' })
    .then(() => window.location.href = '/Turn/EsTuTurno');
}
