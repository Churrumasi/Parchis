# caso_de_uso_6_ejercer_turno

Proyecto ASP.NET Core MVC (.NET 8) demo para el **Caso de Uso CU-06: Ejercer turno** (Parchís - ejecución local, arquitectura EDA en memoria).

## Estructura
- Controllers/TurnController.cs
- Models/Domain/Player.cs, GameState.cs
- Models/Events/*.cs (eventos)
- Services/* (InMemoryEventBus, TurnManager, GameOrchestratorHostedService)
- Views/Turn/* (pantallas EsTuTurno, TirarDado, Inactividad, SeleccionFicha, FinTurno)
- Views/Shared/_Layout.cshtml
- wwwroot/js/turn.js
- Program.cs
- caso_de_uso_6_ejercer_turno.csproj

## Requisitos
- .NET 8 SDK
- Visual Studio 2022/2023 o VS Code con C# extension

## Cómo ejecutar
1. Descomprime el zip en una carpeta.
2. Abre la solución `caso_de_uso_6_ejercer_turno.sln` en Visual Studio.
3. Presiona F5 o ejecuta `dotnet run` desde la carpeta del proyecto.
4. El navegador abrirá la página inicial (ruta `/Turn/EsTuTurno`).

## Flujo de prueba
1. `EsTuTurno` -> pulsa **Avanzar - Tirar dado**.
2. En `Tirar dado`, haz clic en el dado para lanzar (o ve a `Inactividad` y deja expirar 10s para que lance automáticamente).
3. Se mostrará el valor del dado y se redirige a `SeleccionFicha`.
4. Selecciona una ficha (simulada) para mover — el orquestador procesará y pasará turno.
5. Repite para ver el cambio de jugador demo (Ana / Luis).

## Notas
- Este proyecto usa un bus de eventos en memoria (`InMemoryEventBus`) y un orquestador simplificado (`GameOrchestratorHostedService`) para demostrar EDA.
- Para multi-jugador en tiempo real reemplaza la comunicación por SignalR y un bus persistente (RabbitMQ/Kafka).
