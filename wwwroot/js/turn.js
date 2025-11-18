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
    const dice = document.getElementById('dice');
    const resultEl = document.getElementById('dice-result');
    if (dice) {
        dice.classList.add('rolling');
    }

    fetch('/Turn/LanzarDadoAjax', { method: 'POST' })
        .then(r => {
            if (!r.ok) throw new Error('error en la petición');
            return r.json()
        })
        .then(data => {
            if (dice) {
                // breve animación antes de mostrar resultado
                setTimeout(() => dice.classList.remove('rolling'), 650);
            }
            if (resultEl) resultEl.innerText = 'Valor: ' + data.valor;
            // mostrar panel de selección en la misma página
            setTimeout(() => showSelectionInPage(data.valor), 650);
        })
        .catch(err => {
            if (resultEl) resultEl.innerText = 'Error al lanzar el dado';
            console.error(err);
            alert('Error al lanzar dado: ' + err);
        });
}

function showSelectionInPage(valor) {
    const selList = document.getElementById('selection-list');
    if (!selList) return;
    // limpiar y crear botones con el valor del dado (usando data-attributes para delegation)
    selList.innerHTML = '';
    for (let i = 0; i < 4; i++) {
        const btn = document.createElement('button');
        btn.className = 'btn';
        btn.setAttribute('data-action', 'move');
        btn.setAttribute('data-index', String(i));
        btn.innerText = 'Ficha ' + (i + 1) + ' → ' + valor + ' casillas';
        selList.appendChild(btn);
    }
}

// ---------------------- posicionamiento de fichas (mapa procedimental)
function mapPosToPercent(pos) {
    // pos: integer. If negative -> home (we'll return special values)
    // We treat the board as a square. We'll map 0..67 along the perimeter.
    const total = 68;
    if (pos == null) pos = -1;
    if (pos < 0) return { x: 8, y: 8, home: true }; // placeholder - home handled separately

    const t = (pos % total) / total; // 0..1
    // define padding from edge in percent
    const pad = 6; // percent padding
    const inner = 100 - pad * 2; // usable area

    // walk the perimeter clockwise from top-left towards top-right
    const perQuarter = 0.25;
    let xPerc = 0, yPerc = 0;
    if (t < perQuarter) {
        // top edge: left -> right
        const f = t / perQuarter;
        xPerc = pad + inner * f;
        yPerc = pad;
    } else if (t < perQuarter * 2) {
        // right edge: top -> bottom
        const f = (t - perQuarter) / perQuarter;
        xPerc = pad + inner;
        yPerc = pad + inner * f;
    } else if (t < perQuarter * 3) {
        // bottom edge: right -> left
        const f = (t - perQuarter * 2) / perQuarter;
        xPerc = pad + inner * (1 - f);
        yPerc = pad + inner;
    } else {
        // left edge: bottom -> top
        const f = (t - perQuarter * 3) / perQuarter;
        xPerc = pad;
        yPerc = pad + inner * (1 - f);
    }
    return { x: xPerc, y: yPerc, home: false };
}

function colorToHex(color) {
    switch ((color || '').toLowerCase()) {
        case 'azul': return '#2563eb';
        case 'verde': return '#16a34a';
        case 'amarillo': return '#eab308';
        case 'amarillo ': return '#eab308';
        default: return '#ef4444';
    }
}

function getHomeCoordsForColor(color, index) {
    // return percent coords for home positions by color
    // colors: rojo (bottom-left), azul (top-left), amarillo (top-right), verde (bottom-right)
    if (!color) color = 'rojo';
    const offset = index * 5; // spread pieces a bit
    switch (color.toLowerCase()) {
        case 'azul':
            return { x: 12 + offset, y: 12 + offset };
        case 'amarillo':
        case 'amarillo ': // typo-safety
            return { x: 88 - offset, y: 12 + offset };
        case 'verde':
        case 'verde ': // typo-safety
            return { x: 88 - offset, y: 88 - offset };
        default:
            return { x: 12 + offset, y: 88 - offset };
    }
}

function positionPiecesFromData() {
    const board = document.getElementById('board');
    if (!board) return;
    const rect = board.getBoundingClientRect();
    const pieces = board.querySelectorAll('.piece');
    const boardMap = JSON.parse(localStorage.getItem('boardMap') || '{}');
    pieces.forEach(p => {
        const posAttr = p.getAttribute('data-pos');
        const color = p.getAttribute('data-color') || 'rojo';
        const index = parseInt(p.getAttribute('data-index') || '0', 10) || 0;
        const pos = parseInt(posAttr, 10);
        let coords;
        if (isNaN(pos) || pos < 0) {
            const hp = getHomeCoordsForColor(color, index);
            coords = { x: hp.x, y: hp.y };
        } else {
            // si existe mapeo exacto en localStorage, usarlo
            if (boardMap && boardMap.hasOwnProperty(pos)) {
                coords = { x: boardMap[pos].x, y: boardMap[pos].y };
            } else {
                const pct = mapPosToPercent(pos);
                coords = { x: pct.x, y: pct.y };
            }
        }
        // convert percent to pixels relative to board element
        const x = Math.round((coords.x / 100) * rect.width) - (p.offsetWidth / 2);
        const y = Math.round((coords.y / 100) * rect.height) - (p.offsetHeight / 2);
        p.style.left = x + 'px';
        p.style.top = y + 'px';
    });
}

function initUI() {
    // al cargar, obtener el estado de la partida y posicionar fichas
    refreshGameState();
    // configurar listener de clicks para el mapeador
    const board = document.getElementById('board');
    if (board) {
        board.addEventListener('click', (ev) => {
            if (!window._mapperActive) return;
            const rect = board.getBoundingClientRect();
            const x = ev.clientX - rect.left;
            const y = ev.clientY - rect.top;
            const xp = Math.round((x / rect.width) * 10000) / 100; // percent with 2 decimals
            const yp = Math.round((y / rect.height) * 10000) / 100;
            const idx = prompt('Índice de casilla a mapear (entero) para estos coords ' + xp + '%,' + yp + '%');
            if (idx === null) return;
            const n = parseInt(idx, 10);
            if (isNaN(n)) { alert('Índice no válido'); return; }
            const boardMap = JSON.parse(localStorage.getItem('boardMap') || '{}');
            boardMap[n] = { x: xp, y: yp };
            localStorage.setItem('boardMap', JSON.stringify(boardMap));
            alert('Guardado mapeo para índice ' + n + ': ' + xp + '%,' + yp + '%');
        });
    }
    // attach UI event listeners (avoid inline onclick to prevent ReferenceError)
    const btnToggle = document.getElementById('btn-toggle-mapper');
    if (btnToggle) btnToggle.addEventListener('click', toggleMapper);
    // reflect initial state in the indicator if present
    const mapperState = document.getElementById('mapper-state');
    if (mapperState) mapperState.innerText = window._mapperActive ? 'ON' : 'OFF';
    const btnExport = document.getElementById('btn-export-map');
    if (btnExport) btnExport.addEventListener('click', exportMapping);
    const btnPasar = document.getElementById('btn-pasar-turno');
    if (btnPasar) btnPasar.addEventListener('click', pasarTurno);
    const btnShowFin = document.getElementById('btn-show-fin');
    if (btnShowFin) btnShowFin.addEventListener('click', showFinTurno);
    const diceEl = document.getElementById('dice');
    if (diceEl) diceEl.addEventListener('click', lanzarDado);

    // delegation for selection list buttons (initial static buttons without onclick)
    const selList = document.getElementById('selection-list');
    if (selList) {
        selList.addEventListener('click', (ev) => {
            const btn = ev.target.closest('button');
            if (!btn) return;
            const action = btn.getAttribute('data-action');
            if (action === 'move') {
                const idx = parseInt(btn.getAttribute('data-index') || '0', 10) || 0;
                // if the button was created by showSelectionInPage we need to parse the value from text
                const valueAttr = btn.getAttribute('data-value');
                const value = valueAttr ? parseInt(valueAttr, 10) : 1;
                moverFicha(idx, -1, value);
            }
        });
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initUI);
} else {
    // DOM already loaded — initialize immediately
    initUI();
}
window.addEventListener('resize', () => positionPiecesFromData());

function refreshGameState() {
    fetch('/Turn/EstadoPartida')
        .then(r => r.json())
        .then(gs => {
            // renderizar fichas de todos los jugadores para vista en tiempo real
            const piecesContainer = document.getElementById('pieces');
            if (piecesContainer) {
                piecesContainer.innerHTML = '';
                if (gs.Jugadores && gs.Jugadores.length) {
                    gs.Jugadores.forEach((player, pIdx) => {
                        const color = player.ColorFichas || 'rojo';
                        const posiciones = player.PosicionesFichas || [-1, -1, -1, -1];
                        for (let i = 0; i < 4; i++) {
                            const pos = posiciones[i] == null ? -1 : posiciones[i];
                            const div = document.createElement('div');
                            div.className = 'piece player-' + pIdx;
                            div.setAttribute('data-player', String(pIdx));
                            div.setAttribute('data-index', String(i));
                            div.setAttribute('data-pos', String(pos));
                            div.setAttribute('data-color', color);
                            div.innerText = String(i + 1);
                            // small border to differentiate
                            div.style.background = colorToHex(color);
                            piecesContainer.appendChild(div);
                        }
                    });
                }
            }
            positionPiecesFromData();
            const currentIndex = gs.IndiceJugadorActual || 0;
            const jugador = (gs.Jugadores && gs.Jugadores[currentIndex]) || null;
            const nameEls = document.querySelectorAll('.player-name');
            nameEls.forEach(el => el.innerText = jugador?.Nombre || 'Jugador');
            const finText = document.getElementById('fin-text');
            if (finText) finText.innerText = 'Jugador actual: ' + (jugador?.Nombre ?? '---');
        })
        .catch(err => console.error('No se pudo refrescar el estado', err));
}

function moverFicha(indiceFicha, desde, hasta) {
    fetch('/Turn/MoverFichaAjax', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `indiceFicha=${indiceFicha}&desde=${desde}&hasta=${hasta}`
    }).then(r => r.json())
      .then(data => {
          if (data.ok) {
              // refrescar estado local para reflejar movimiento y fin de turno
              setTimeout(() => refreshGameState(), 120);
          }
      }).catch(err => console.error(err));
}

function pasarTurno() {
    fetch('/Turn/PasarTurnoAjax', { method: 'POST' })
    .then(r => r.json())
    .then(() => refreshGameState())
    .catch(err => console.error(err));
}

// ---------------------- mapeador de tablero (localStorage)
function toggleMapper() {
    window._mapperActive = !window._mapperActive;
    const stateEl = document.getElementById('mapper-state');
    const btn = document.getElementById('btn-toggle-mapper');
    if (stateEl) stateEl.innerText = window._mapperActive ? 'ON' : 'OFF';
    if (btn) {
        btn.classList.toggle('active', !!window._mapperActive);
        btn.innerText = window._mapperActive ? 'Mapear (ON)' : 'Mapear tablero';
    }
    // pequeña notificación no intrusiva
    console.log('Modo mapeador ' + (window._mapperActive ? 'ACTIVO' : 'DESACTIVADO'));
    if (window._mapperActive) {
        // foco visual
        const board = document.getElementById('board');
        if (board) board.style.outline = '3px dashed rgba(59,130,246,0.8)';
    } else {
        const board = document.getElementById('board');
        if (board) board.style.outline = 'none';
    }
}

function exportMapping() {
    const boardMap = JSON.parse(localStorage.getItem('boardMap') || '{}');
    const dataStr = 'data:text/json;charset=utf-8,' + encodeURIComponent(JSON.stringify(boardMap, null, 2));
    const a = document.createElement('a');
    a.setAttribute('href', dataStr);
    a.setAttribute('download', 'board-map.json');
    document.body.appendChild(a);
    a.click();
    a.remove();
}

function showFinTurno() {
    const overlay = document.getElementById('fin-overlay');
    if (overlay) overlay.style.display = 'flex';
}

function closeFin() {
    const overlay = document.getElementById('fin-overlay');
    if (overlay) overlay.style.display = 'none';
}
