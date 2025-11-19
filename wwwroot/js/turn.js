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
        btn.type = 'button';
        btn.className = 'btn';
        btn.setAttribute('data-action', 'move');
        btn.setAttribute('data-index', String(i));
        // store the dado value so the delegated handler knows how many casillas
        btn.setAttribute('data-value', String(valor));
        btn.innerText = 'Ficha ' + (i + 1) + ' → ' + valor + ' casillas';
        selList.appendChild(btn);
    }
}

// ---------------------- posicionamiento de fichas (mapeo real del tablero)
const BOARD_MAP = {
  "1": { "x": 94.62, "y": 41.14 }, "2": { "x": 89.56, "y": 40.51 }, "3": { "x": 85.13, "y": 40.38 },
  "4": { "x": 80.96, "y": 40.26 }, "5": { "x": 75.77, "y": 39.75 }, "6": { "x": 71.22, "y": 39.75 },
  "7": { "x": 66.03, "y": 39.75 }, "8": { "x": 61.48, "y": 40.76 }, "9": { "x": 57.68, "y": 37.22 },
  "10": { "x": 59.07, "y": 33.43 }, "11": { "x": 59.83, "y": 27.99 }, "12": { "x": 59.2, "y": 23.18 },
  "13": { "x": 59.2, "y": 19 }, "14": { "x": 58.94, "y": 14.07 }, "15": { "x": 59.2, "y": 9.64 },
  "16": { "x": 58.69, "y": 4.07 }, "17": { "x": 49.96, "y": 4.71 }, "18": { "x": 40.73, "y": 4.71 },
  "19": { "x": 40.6, "y": 10.02 }, "20": { "x": 40.35, "y": 14.2 }, "21": { "x": 40.6, "y": 18.75 },
  "22": { "x": 40.98, "y": 24.7 }, "23": { "x": 40.73, "y": 27.99 }, "24": { "x": 40.73, "y": 32.92 },
  "25": { "x": 42.5, "y": 36.97 }, "26": { "x": 38.32, "y": 41.65 }, "27": { "x": 33.14, "y": 41.78 },
  "28": { "x": 28.7, "y": 41.44 }, "29": { "x": 24.02, "y": 40.4 }, "30": { "x": 18.96, "y": 40.53 },
  "31": { "x": 14.29, "y": 40.27 }, "32": { "x": 9.75, "y": 40.01 }, "33": { "x": 4.17, "y": 40.53 },
  "34": { "x": 4.95, "y": 50.13 }, "35": { "x": 5.08, "y": 59.21 }, "36": { "x": 10.01, "y": 58.82 },
  "37": { "x": 13.9, "y": 58.95 }, "38": { "x": 19.22, "y": 59.34 }, "39": { "x": 24.15, "y": 58.95 },
  "40": { "x": 29.08, "y": 58.3 }, "41": { "x": 32.98, "y": 57.92 }, "42": { "x": 37.39, "y": 57.4 },
  "43": { "x": 42.19, "y": 61.81 }, "44": { "x": 40.76, "y": 65.96 }, "45": { "x": 39.98, "y": 71.28 },
  "46": { "x": 41.15, "y": 75.17 }, "47": { "x": 40.76, "y": 79.85 }, "48": { "x": 40.5, "y": 84.52 },
  "49": { "x": 40.11, "y": 89.32 }, "50": { "x": 39.85, "y": 94.25 }, "51": { "x": 50.11, "y": 94.25 },
  "52": { "x": 59.84, "y": 94.64 }, "53": { "x": 59.32, "y": 89.97 }, "54": { "x": 58.41, "y": 84.91 },
  "55": { "x": 59.58, "y": 80.88 }, "56": { "x": 58.8, "y": 75.17 }, "57": { "x": 58.02, "y": 71.02 },
  "58": { "x": 59.45, "y": 65.96 }, "59": { "x": 58.02, "y": 62.07 }, "60": { "x": 61.4, "y": 57.92 },
  "61": { "x": 66.07, "y": 58.43 }, "62": { "x": 71.26, "y": 58.05 }, "63": { "x": 75.67, "y": 58.18 },
  "64": { "x": 80.99, "y": 58.69 }, "65": { "x": 85.53, "y": 58.56 }, "66": { "x": 89.68, "y": 58.82 },
  "67": { "x": 94.87, "y": 58.3 }, "68": { "x": 95.13, "y": 49.74 }
};

// Starting positions for each color when leaving home
const COLOR_START_POS = {
    'amarillo': 4,
    'azul': 12,
    'rojo': 38,
    'verde': 55
};

function mapPosToPercent(pos) {
    // pos: integer position on board (1..68) or negative for home
    if (pos == null || pos < 0) return null;
    const key = String(pos);
    return BOARD_MAP[key] || null;
}

function getHomeCoordsForColor(color, index) {
    // return percent coords for home positions by color (in their respective corners)
    // Layout: azul (top-left), amarillo (top-right), rojo (bottom-left), verde (bottom-right)
    if (!color) color = 'rojo';
    const offset = index * 3; // spread pieces slightly
    switch (color.toLowerCase()) {
        case 'azul':
            return { x: 15 + offset, y: 15 + offset };
        case 'amarillo':
            return { x: 85 - offset, y: 15 + offset };
        case 'verde':
            return { x: 85 - offset, y: 85 - offset };
        case 'rojo':
        default:
            return { x: 15 + offset, y: 85 - offset };
    }
}

function positionPiecesFromData() {
    const board = document.getElementById('board');
    if (!board) return;
    const rect = board.getBoundingClientRect();
    const pieces = board.querySelectorAll('.piece');
    pieces.forEach(p => {
        const posAttr = p.getAttribute('data-pos');
        const color = p.getAttribute('data-color') || 'rojo';
        const index = parseInt(p.getAttribute('data-index') || '0', 10) || 0;
        const pos = parseInt(posAttr, 10);
        let coords;
        if (isNaN(pos) || pos < 0) {
            // piece is at home
            const hp = getHomeCoordsForColor(color, index);
            coords = { x: hp.x, y: hp.y };
        } else {
            // piece is on the board
            const pct = mapPosToPercent(pos);
            if (pct) {
                coords = { x: pct.x, y: pct.y };
            } else {
                // fallback if position not in map
                const hp = getHomeCoordsForColor(color, index);
                coords = { x: hp.x, y: hp.y };
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
    // attach UI event listeners (avoid inline onclick to prevent ReferenceError)
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
            console.log('selection-list click', action, btn.getAttribute('data-index'));
            if (action === 'move') {
                const idx = parseInt(btn.getAttribute('data-index') || '0', 10) || 0;
                const valueAttr = btn.getAttribute('data-value');
                const value = valueAttr ? parseInt(valueAttr, 10) : 1;
                moverFicha(idx, -1, value);
            }
        });
    }

    // start periodic refresh to keep UI in sync with server-side turns
    if (!window._refreshInterval) {
        window._refreshInterval = setInterval(() => {
            try { refreshGameState(); } catch (e) { console.error('Periodic refresh error', e); }
        }, 1000);
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
            console.log('Estado recibido:', gs);
            const currentIndex = (typeof gs.IndiceJugadorActual === 'number') ? gs.IndiceJugadorActual : 0;
            const lastIndex = window._lastIndiceJugadorActual;
            if (lastIndex !== undefined && lastIndex !== currentIndex) {
                console.log('Cambio de turno detectado', { from: lastIndex, to: currentIndex });
                // mostrar brevemente overlay de fin de turno
                showFinTurno();
                setTimeout(() => closeFin(), 800);
            }
            window._lastIndiceJugadorActual = currentIndex;
            const jugador = (gs.Jugadores && gs.Jugadores[currentIndex]) || null;
            if (jugador) {
                const nameEls = document.querySelectorAll('.player-name');
                nameEls.forEach(el => el.innerText = jugador.Nombre);
            }

            // Renderizar fichas de TODOS los jugadores
            const board = document.getElementById('board');
            const piecesContainer = document.getElementById('pieces');
            console.log('Contenedores:', { board, piecesContainer, jugadores: gs.Jugadores?.length });
            if (board && piecesContainer && gs.Jugadores) {
                // Limpiar fichas existentes
                piecesContainer.innerHTML = '';
                console.log('Renderizando fichas para', gs.Jugadores.length, 'jugadores');
                
                // Crear fichas para cada jugador
                gs.Jugadores.forEach((player, playerIdx) => {
                    const color = player.ColorFichas || 'rojo';
                    const positions = player.PosicionesFichas || [];
                    console.log(`Jugador ${playerIdx} (${player.Nombre}):`, { color, positions });
                    // Mapear color a CSS background
                    const bgColor = color === 'azul' ? '#2563eb' : 
                                   color === 'verde' ? '#16a34a' : 
                                   color === 'amarillo' ? '#eab308' : '#ef4444';
                    
                    positions.forEach((pos, idx) => {
                        const pieceEl = document.createElement('div');
                        pieceEl.className = 'piece';
                        pieceEl.style.background = bgColor;
                        pieceEl.setAttribute('data-pos', pos != null ? pos : -1);
                        pieceEl.setAttribute('data-color', color);
                        pieceEl.setAttribute('data-index', idx);
                        pieceEl.setAttribute('data-player-index', playerIdx);
                        pieceEl.innerText = (idx + 1).toString();
                        
                        // Añadir borde más grueso para el jugador actual
                        if (playerIdx === currentIndex) {
                            pieceEl.style.border = '3px solid white';
                            pieceEl.style.boxShadow = '0 0 8px rgba(255,255,255,0.8)';
                        }
                        
                        piecesContainer.appendChild(pieceEl);
                        console.log(`  Ficha ${idx+1} creada en posición ${pos}`);
                    });
                });
                console.log('Total fichas creadas:', piecesContainer.children.length);
            }
            
            positionPiecesFromData();
            const finText = document.getElementById('fin-text');
            if (finText) finText.innerText = 'Jugador actual: ' + (jugador?.Nombre ?? '---');
        })
        .catch(err => console.error('No se pudo refrescar el estado', err));
}

function moverFicha(indiceFicha, desde, hasta) {
    console.log('moverFicha()', { indiceFicha, desde, hasta });
    // disable selection buttons while request is in flight
    const selList = document.getElementById('selection-list');
    const buttons = selList ? Array.from(selList.querySelectorAll('button')) : [];
    buttons.forEach(b => b.disabled = true);

    fetch('/Turn/MoverFichaAjax', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `indiceFicha=${encodeURIComponent(indiceFicha)}&desde=${encodeURIComponent(desde)}&hasta=${encodeURIComponent(hasta)}`
    })
    .then(r => {
        console.log('moverFicha response status', r.status, r.statusText);
        const ct = r.headers.get('content-type') || '';
        if (!r.ok) return r.text().then(t => { throw new Error('Server error: ' + t); });
        if (ct.indexOf('application/json') !== -1) return r.json();
        return r.text();
    })
    .then(data => {
        console.log('moverFicha response data', data);
        if (data && data.ok) {
            // refrescar estado local para reflejar movimiento y fin de turno
            setTimeout(() => refreshGameState(), 120);
        } else if (typeof data === 'string') {
            console.warn('moverFicha returned text:', data);
            // intentar refrescar igualmente
            setTimeout(() => refreshGameState(), 150);
        }
    })
    .catch(err => {
        console.error('Error en moverFicha:', err);
        alert('Error al mover ficha: ' + (err && err.message ? err.message : err));
    })
    .finally(() => {
        buttons.forEach(b => b.disabled = false);
    });
}

function pasarTurno() {
    const btn = document.getElementById('btn-pasar-turno');
    if (btn) btn.disabled = true;
    console.log('Pasar turno requested');
    fetch('/Turn/PasarTurnoAjax', { method: 'POST' })
    .then(r => {
        console.log('pasarTurno response status', r.status, r.statusText);
        if (!r.ok) return r.text().then(t => { throw new Error('Server error: ' + t); });
        const ct = r.headers.get('content-type') || '';
        if (ct.indexOf('application/json') !== -1) return r.json();
        return r.text();
    })
    .then(data => {
        console.log('pasarTurno response data', data);
        refreshGameState();
    })
    .catch(err => {
        console.error('Error en pasarTurno:', err);
        alert('Error al pasar turno: ' + (err && err.message ? err.message : err));
    })
    .finally(() => { if (btn) btn.disabled = false; });
}

// mapping removed — kept procedural mapper `mapPosToPercent` for placement

function showFinTurno() {
    const overlay = document.getElementById('fin-overlay');
    if (overlay) overlay.style.display = 'flex';
}

function closeFin() {
    const overlay = document.getElementById('fin-overlay');
    if (overlay) overlay.style.display = 'none';
}

