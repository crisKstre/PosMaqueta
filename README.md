# Sistema POS — Maqueta base

Esqueleto base de un punto de venta (POS) de escritorio en **C# / WinForms**,
con arquitectura en capas y base de datos **SQLite local** (sin servidor).

## Requisitos

- Visual Studio 2022 (o 2019) con **.NET Framework 4.7.2**.
- Nada más que instalar: SQLite va embebido vía el paquete NuGet
  `Microsoft.Data.Sqlite`, que se descarga solo al compilar.
- (Opcional) [DB Browser for SQLite](https://sqlitebrowser.org/) para
  inspeccionar el archivo `pos.db` a mano.

## Cómo arrancar

1. Abre `PosMaqueta.sln` en Visual Studio.
2. Deja que restaure los paquetes NuGet (lo hace solo).
3. Proyecto de inicio: **Presentacion** (clic derecho → Establecer como
   proyecto de inicio si no lo está).
4. Ejecuta (F5).

Al arrancar por primera vez crea el archivo `pos.db` junto al `.exe`,
con todas las tablas y un usuario administrador por defecto.

### Credenciales por defecto

| Usuario | Contraseña |
|---------|-----------|
| `admin` | `admin123` |

## Arquitectura (capas)

```
Presentacion  (WinForms: login, shell con sidebar, forms de módulos)
     │
Dominio       (Servicios = lógica de negocio, Eventos = observer)
     │
AccesoData    (DAOs con SQL, conexión SQLite, inicializador de BD)
     │
Entidades     (modelos: Usuario, Producto, Venta, DetalleVenta, Caja)
```

Cada **módulo** futuro (Productos, Ventas, Caja) se arma con 4 archivos,
uno por capa, sin tocar el resto.

## Qué incluye esta versión

**Núcleo**
- Login con roles (admin / cajero) y hash de contraseña SHA256.
- Conexión SQLite + creación automática de la base de datos.
- Shell principal con sidebar y navegación entre módulos.
- Control de acceso por rol.
- Sistema de notificación de cambios (`NotificadorCambios`) — patrón observer.

**Módulo Productos**
- CRUD completo (crear, editar, eliminar, desactivar).
- Búsqueda en vivo por nombre o código de barras.
- Control de inventario: entrada (+) y salida (−) de stock.
- Alerta de stock bajo el mínimo.
- Soporte por unidad o por peso (kg).

**Módulo Ventas**
- Carrito con lector de código de barras (emulación de teclado).
- Búsqueda por nombre con sugerencias en vivo.
- Cálculo de total, descuento automático de stock.
- Registro de la venta en transacción (cabecera + detalle).
- Atajo F12 para cobrar.

**Módulo Caja**
- Apertura de turno con monto inicial (solo admin).
- Resumen del turno: cantidad de ventas, total vendido, desglose por medio de pago.
- Cierre con arqueo: compara efectivo esperado vs. real contado.
- Las ventas se asocian automáticamente a la caja abierta.

## Lo que sigue (v2)

- Clientes, integración Mercado Pago Point, reportes con factura electrónica (DTE).

## Notas

- La contraseña se guarda como hash SHA256. Para producción conviene migrar
  a PBKDF2 con salt por usuario.
- Las boletas las emite la máquina de pago (Mercado Pago Point u otra), el
  sistema no genera comprobantes — solo registra la venta internamente.
