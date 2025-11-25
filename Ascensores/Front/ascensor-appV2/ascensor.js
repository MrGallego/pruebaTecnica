angular.module('ascensorApp', [])
  .service('AscensorService', ['$http', '$interval', function($http, $interval) {
    const service = this;

    service.estado = {}; // inicial vacío
    service.solicitudes = [];

    service.cargarEstado = function() {
      return $http.get('https://localhost:7023/api/Ascensor')
        .then(function(response) {
          Object.assign(service.estado, response.data);
        });
    };

    service.cargarSolicitudes = function() {
      return $http.get('https://localhost:7023/api/Solicitudes')
        .then(function(response) {
          service.solicitudes.length = 0;
          Array.prototype.push.apply(service.solicitudes, response.data);
        });
    };

   service.solicitarPiso = function(piso) {
  if (piso === null || piso === undefined) return;
  return $http.post('https://localhost:7023/api/Solicitudes', { piso: piso })
    .then(() => service.cargarSolicitudes());
};


    service.abrir = function() { return $http.post('https://localhost:7023/api/Ascensor/abrir', {}); };
    service.cerrar = function() { return $http.post('https://localhost:7023/api/Ascensor/cerrar', {}); };
    service.iniciar = function() { return $http.post('https://localhost:7023/api/Ascensor/iniciar', {}); };
    service.detener = function() { return $http.post('https://localhost:7023/api/Ascensor/detener', {}); };
  }])
  .controller('AscensorController', ['AscensorService', '$interval', function(AscensorService, $interval) {
    const ctrl = this;

    ctrl.estado = AscensorService.estado;
    ctrl.solicitudes = AscensorService.solicitudes;
    ctrl.pisoSolicitado = null;

    AscensorService.cargarEstado();
    AscensorService.cargarSolicitudes();

    // Auto-actualizar cada segundo
    $interval(function() {
      AscensorService.cargarEstado();
      AscensorService.cargarSolicitudes();
    }, 1000);

    ctrl.solicitarPiso = function() {
      AscensorService.solicitarPiso(ctrl.pisoSolicitado)
        .finally(() => { ctrl.pisoSolicitado = null; });
    };

    ctrl.abrir = function() { AscensorService.abrir(); };
    ctrl.cerrar = function() { AscensorService.cerrar(); };
    ctrl.iniciar = function() { AscensorService.iniciar(); };
    ctrl.detener = function() { AscensorService.detener(); };
  }]);
