using Ascensores.Data.Helper;
using Ascensores.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Concurrent;

namespace Ascensores.Services
{

    public class AscensorService : IHostedService, IDisposable
    {
        private readonly OracleHelper _db;
        private readonly int _minPiso = 1;
        private readonly int _maxPiso = 20;
        private readonly int _segundosPorPiso = 2; // configurable
        private readonly TimeSpan _delayPorPiso;

        private Ascensor _ascensor = new Ascensor();
        private readonly ConcurrentQueue<int> _colaSolicitudes = new ConcurrentQueue<int>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task? _processingTask;
        private readonly object _lock = new object();

        public AscensorService(OracleHelper db)
        {
            _db = db;
            _delayPorPiso = TimeSpan.FromSeconds(_segundosPorPiso);
        }

        // IHostedService
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Cargar solicitudes pendientes desde BD
            _ = LoadSolicitudesPendientes();
            _processingTask = Task.Run(() => ProcessLoop(_cts.Token));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        // API exposada
        public Ascensor GetEstado() => _ascensor;

        public async Task<List<Solicitud>> GetSolicitudesPendientes()
        {
            var sql = "SELECT id, piso, atendida, fecha_solicitud FROM solicitudes WHERE atendida = 0 ORDER BY fecha_solicitud";
            return await _db.QueryAsync(sql, reader => new Solicitud
            {
                Id = Convert.ToInt32(reader[0]),
                Piso = Convert.ToInt32(reader[1]),
                Atendida = Convert.ToInt32(reader[2]) == 1,
                FechaSolicitud = Convert.ToDateTime(reader[3])
            });
        }

        public async Task<int> CrearSolicitud(int piso)
        {
            if (piso < _minPiso || piso > _maxPiso) throw new ArgumentOutOfRangeException(nameof(piso));
            var sql = "INSERT INTO solicitudes (piso) VALUES (:piso)";
            var p = new OracleParameter(":piso", OracleDbType.Int32) { Value = piso };
            await _db.ExecuteNonQueryAsync(sql, p);
            // Encolar localmente para respuesta inmediata
            _colaSolicitudes.Enqueue(piso);
            return 1;
        }

        public async Task AbrirPuertasAsync()
        {
            lock (_lock) { _ascensor.PuertasAbiertas = true; }
            var sql = "INSERT INTO log_operaciones (descripcion) VALUES (:d)";
            await _db.ExecuteNonQueryAsync(sql, new OracleParameter(":d", $"Puertas abiertas en piso {_ascensor.PisoActual}"));
        }

        public async Task CerrarPuertasAsync()
        {
            lock (_lock) { _ascensor.PuertasAbiertas = false; }
            var sql = "INSERT INTO log_operaciones (descripcion) VALUES (:d)";
            await _db.ExecuteNonQueryAsync(sql, new OracleParameter(":d", $"Puertas cerradas en piso {_ascensor.PisoActual}"));
        }

        public Task IniciarAsync()
        {
            lock (_lock) { 
                _ascensor.EnMovimiento = true;
                _ascensor.PuertasAbiertas = false;
            }
            return Task.CompletedTask;
        }

        public Task DetenerAsync()
        {
            lock (_lock) { _ascensor.EnMovimiento = false; _ascensor.Direccion = "DETENIDO"; }
            return Task.CompletedTask;
        }

        // Carga pendientes al iniciar
        private async Task LoadSolicitudesPendientes()
        {
            var pendientes = await GetSolicitudesPendientes();
            foreach (var s in pendientes) _colaSolicitudes.Enqueue(s.Piso);
        }

        // Loop principal de procesamiento (background)
        private async Task ProcessLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!_colaSolicitudes.TryPeek(out int pisoDestino))
                {
                    _ascensor.Direccion = "DETENIDO";
                    _ascensor.EnMovimiento = false;
                    await Task.Delay(500, token);
                    continue;
                }
                if (!_ascensor.EnMovimiento)
                {
                    _ascensor.Direccion = "DETENIDO";
                    await Task.Delay(500, token);
                    continue;
                }

                if (_ascensor.PisoActual == pisoDestino)
                {
                    // Llegó
                    _ascensor.EnMovimiento = false;
                    _ascensor.PuertasAbiertas = true;
                    _ascensor.Direccion = "DETENIDO";

                    if (_colaSolicitudes.TryDequeue(out int pisoAtendido))
                    {
                        await _db.ExecuteNonQueryAsync(
                            "UPDATE solicitudes SET atendida = 1 WHERE piso = :piso AND atendida = 0",
                            new OracleParameter(":piso", pisoAtendido)
                        );
                    }
                    await Task.Delay(2000, token); // puertas abiertas
                    _ascensor.PuertasAbiertas = false;
                    continue;
                }

                //Actualizar dirección según el movimiento
                if (_ascensor.PisoActual < pisoDestino)
                    _ascensor.Direccion = "SUBIENDO";
                else
                    _ascensor.Direccion = "BAJANDO";

                _ascensor.EnMovimiento = true;

                // Simular movimiento 1 piso
                await Task.Delay(_delayPorPiso, token);

                // Mover 1 piso
                if (_ascensor.Direccion == "SUBIENDO")
                    _ascensor.PisoActual++;
                else
                    _ascensor.PisoActual--;
            }
        }
    }
}