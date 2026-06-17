# Manual de Usuario — Sistema POS

Guía de operación del punto de venta para **cajeros** y **administradores**.

> El sistema es de **uso interno** del negocio (no lo opera el cliente). La boleta/comprobante
> la emite una máquina aparte; aquí se **registra y controla** la venta.

## Índice

1. [Iniciar sesión](#1-iniciar-sesión)
2. [Roles y permisos](#2-roles-y-permisos)
3. [La pantalla principal](#3-la-pantalla-principal)
4. [Inicio (Dashboard)](#4-inicio-dashboard)
5. [Ventas](#5-ventas)
6. [Productos (Inventario)](#6-productos-inventario)
7. [Caja](#7-caja)
8. [Reportes](#8-reportes)
9. [Usuarios y contraseñas](#9-usuarios-y-contraseñas)
10. [Respaldos](#10-respaldos)
11. [Atajos de teclado](#11-atajos-de-teclado)
12. [Preguntas frecuentes](#12-preguntas-frecuentes)

---

## 1. Iniciar sesión

Al abrir el programa aparece la pantalla de **inicio de sesión**. Ingresa tu **usuario** y
**contraseña** y presiona **Entrar**.

Usuarios de prueba que vienen configurados:

| Usuario     | Contraseña     | Rol           |
|-------------|----------------|---------------|
| `admin`     | `admin123`     | Administrador |
| `empleado`  | `empleado123`  | Cajero        |

> Los usuarios los configura quien instala el sistema. Las contraseñas se guardan cifradas
> (PBKDF2 con sal); nunca se almacenan en texto plano.

Si la contraseña es incorrecta, el sistema lo avisa y el intento queda registrado en el log.

**Cambio obligatorio:** la primera vez que entras con una contraseña asignada —el `admin` por
defecto, una cuenta recién creada o una **reseteada** por un administrador— el sistema te pide
**definir una contraseña nueva** (distinta de la anterior, mínimo 6 caracteres) antes de continuar.

---

## 2. Roles y permisos

Hay dos roles. El sistema **oculta** lo que cada rol no puede usar.

| Acción | Administrador | Cajero (empleado) |
|---|:---:|:---:|
| Vender (módulo Ventas) | ✅ | ✅ |
| Ver inventario | ✅ | ✅ (solo lectura) |
| Crear / editar / eliminar productos | ✅ | ❌ |
| Ajustar stock, descuentos por producto | ✅ | ❌ |
| Abrir / cerrar caja | ✅ | ❌ (solo ve el estado) |
| Histórico de cajas | ✅ | ❌ |
| Módulo Reportes | ✅ | ❌ (no aparece) |
| Autorizar un cierre con faltante | ✅ | ❌ |
| Gestionar usuarios (crear, editar, resetear contraseñas) | ✅ | ❌ (no aparece) |
| Cambiar su propia contraseña | ✅ | ✅ |
| Respaldos y restauración de la base | ✅ | ❌ (no aparece) |

---

## 3. La pantalla principal

Tras iniciar sesión ves la **barra lateral** (sidebar) a la izquierda con los módulos, tu
nombre/rol arriba y el reloj. Haz clic en un módulo para abrirlo, o usa **Ctrl + 1…5**:

| Atajo | Módulo |
|---|---|
| **Ctrl + 1** | Inicio |
| **Ctrl + 2** | Ventas |
| **Ctrl + 3** | Productos |
| **Ctrl + 4** | Caja |
| **Ctrl + 5** | Reportes *(solo admin)* |
| **Ctrl + 6** | Usuarios *(solo admin)* |
| **Ctrl + 7** | Respaldos *(solo admin)* |

Abajo del todo están **🔑 Cambiar contraseña** (cualquier usuario) y **Cerrar sesión**, que
vuelve a la pantalla de login (y descarta las ventas en curso del cajero).

---

## 4. Inicio (Dashboard)

Resumen del día de un vistazo:

- **Ventas de hoy** (cantidad) y **total vendido**.
- **Productos bajo stock** (cuántos están en o por debajo de su mínimo).
- **Estado de la caja** (abierta/cerrada).
- Tablas con las **ventas de hoy** y los **productos bajo stock**.

Se actualiza solo cuando ocurre una venta o un cambio de stock. **F5** lo refresca a mano.

---

## 5. Ventas

Es la pantalla principal del cajero. Flujo típico: **agregar productos → cobrar**.

### Agregar productos al carrito

Tienes tres formas:

1. **Escáner de código de barras** — el lector escribe el código en el campo *Código de
   barras* y agrega el producto al presionar Enter (el lector lo hace solo). Atajo para
   poner el cursor ahí: **F3**.
2. **Buscar por nombre** — escribe en *Buscar por nombre* (atajo **F2**). Las tarjetas de
   productos de abajo se **filtran** mientras escribes. Presiona **Enter** para agregar la
   mejor coincidencia.
3. **Tarjetas de producto** — haz clic en una tarjeta para agregarla. Se agrega la cantidad
   indicada en el campo *Cantidad* (1 por defecto).

Una tarjeta con **oferta** muestra el precio rebajado en verde, el precio original tachado y
un distintivo **−X %**.

### El carrito

Cada línea muestra el producto, la cantidad, el subtotal y tres acciones:

- **−** restar una unidad · **+** sumar una unidad · **✕** quitar la línea.

Si el producto tiene descuento, la línea muestra **(−X %)** y el subtotal ya viene rebajado.

### Varias ventas a la vez

Puedes atender más de una venta en paralelo (p. ej. dejar una en pausa para cobrar a otra
persona):

- Las **pestañas** arriba del carrito son las ventas abiertas (*Venta 1*, *Venta 2*, …).
- El botón **+** crea una **nueva venta** (atajo **F6**). Haz clic en una pestaña para volver
  a esa venta.
- Una venta **en pausa** (no es la que estás viendo) se **cierra sola** si pasa **10 minutos**
  sin usarse. La venta que tienes en primer plano nunca se cierra sola.

### Descuento sobre el total

Botón **Descuento** (atajo **F4**): ingresa un monto en pesos a descontar del total. Déjalo
**vacío** para quitarlo. El descuento se ajusta solo si después quitas productos (nunca queda
mayor que el subtotal).

> El **descuento por producto** (porcentaje) lo configura el administrador en *Productos*; se
> aplica automáticamente al vender. Ambos descuentos se combinan.

### Total e IVA

Bajo el total se muestra el desglose **Neto + IVA 19 %**. En Chile los precios **incluyen
IVA**: el sistema separa cuánto es neto y cuánto es impuesto, de forma que siempre cuadra.

### Cobrar

1. Elige el **medio de pago**: *Efectivo*, *Tarjeta* o *Transferencia*.
2. Presiona **Cobrar** (atajo **F12**).
3. Si es **Efectivo**, se abre un diálogo donde ingresas **con cuánto paga** el cliente y el
   sistema calcula el **vuelto** (con montos rápidos).
4. Aparece el comprobante interno con el N° de venta, total, neto/IVA y el vuelto.

Al cobrar, se **descuenta el stock** y se registra la venta en una sola operación. Antes de
registrar, el sistema **re-valida el stock**: si otro cajero o ajuste lo dejó sin existencias,
avisa y no completa la venta.

### Cancelar una venta

Botón **Cancelar** (atajo **Esc**): descarta la venta en curso (pide confirmación si tiene
productos).

### Registro de ventas

Botón **▼ Registro de ventas**: muestra las ventas del período. **Doble clic** en una venta
abre su **detalle por código** (cada ítem con su código de barras, cantidad, precio,
descuento y subtotal).

### Atajos de Ventas

| Tecla | Acción |
|---|---|
| **F2** | Ir a *Buscar por nombre* |
| **F3** | Ir a *Código de barras* (escáner) |
| **F4** | Descuento al total |
| **F6** | Nueva venta |
| **Esc** | Cancelar venta |
| **F12** | Cobrar |

---

## 6. Productos (Inventario)

### Para el cajero (solo lectura)

Ve la **tabla de productos** (código, nombre, categoría, precio, descuento, stock y estado),
puede **buscar** (**F2**) y ver el **log de inventario**. No puede modificar nada.

### Para el administrador

Además puede:

- **Crear / editar** un producto con el formulario superior (código, nombre, categoría,
  precio, stock, stock mínimo, unidad). **Doble clic** en una fila lo carga para editar. Al
  actualizar se pide confirmación.
- **Gestionar categorías** (botón junto a *Categoría*): agregar o eliminar categorías.
- **Ajustar stock**: selecciona un producto, escribe una cantidad y usa **Agregar stock** o
  **Descontar stock**.
- **Activar / Desactivar** un producto (desactivado queda en gris y no se puede vender, pero
  conserva su historial).
- **Eliminar** un producto. *Solo se puede si no tiene ventas registradas*; si las tiene, usa
  **Desactivar** para conservar el historial.
- **Descuento por producto (oferta):** **clic derecho** sobre la fila → menú → **Aplicar /
  editar descuento** (ingresas el % en un diálogo) o **Quitar descuento**. La columna *Desc.*
  muestra el porcentaje vigente. La oferta se aplica sola al vender ese producto.

El estado de cada producto se ve con color: **OK** (verde), **Bajo** (rojo, en o bajo el
mínimo) e **Inactivo** (gris).

### Atajos de Productos

| Tecla | Acción |
|---|---|
| **F2** | Ir al buscador |
| **F5** | Recargar la tabla |
| **Esc** | Limpiar el formulario *(admin)* |

---

## 7. Caja

Controla el efectivo del turno.

### Abrir caja (admin)

Ingresa el **monto inicial** (fondo de caja) y abre el turno. A partir de ahí, las ventas en
efectivo se asocian a esa caja.

### Cerrar caja con arqueo

Al cerrar, el sistema calcula el **efectivo esperado** (fondo inicial + ventas en efectivo) y
te pide el **monto contado** físicamente. Muestra la **diferencia**:

- **Cuadrada** (diferencia 0), **sobrante** (de más) o **faltante** (de menos).
- Un **faltante** requiere **autorización de un administrador** (se pide usuario y contraseña
  de admin) para poder cerrar.

### Histórico de cajas (admin)

Botón **Histórico de cajas**: lista todos los turnos con apertura, cierre, usuario, montos y
diferencia de arqueo, con **filtros por fecha** (Hoy / 7 días / Mes / rango).

**F5** refresca el estado de la caja.

---

## 8. Reportes *(solo administrador)*

Análisis del negocio por período.

- **Filtros de fecha**: *Desde / Hasta* y accesos rápidos **Hoy / 7 días / Mes**.
- **Resumen**: cantidad de ventas, total vendido, **ticket promedio**, IVA y **desglose por
  medio de pago** (efectivo / tarjeta / transferencia).
- **Productos más vendidos** (ranking).
- **Listado de ventas**: **doble clic** abre el detalle por código. El botón **Anular venta**
  revierte una venta: **devuelve el stock** al inventario y la excluye de los reportes (queda
  auditada). Anular dos veces no duplica el stock.

**F5** vuelve a generar el reporte con el rango actual.

---

## 9. Usuarios y contraseñas

### Cambiar mi contraseña (cualquier usuario)

En la barra lateral, abajo, el botón **🔑 Cambiar contraseña** abre un diálogo donde ingresas tu
**contraseña actual** y la **nueva** (repetida para confirmar). La nueva debe tener al menos 6
caracteres y ser distinta de la actual.

### Gestión de usuarios *(solo administrador)*

Módulo **👤 Usuarios** del menú lateral (**Ctrl + 6**). Permite:

- **Crear** un usuario: nombre, *usuario* (login), **rol** (Administrador o Cajero) y una
  contraseña inicial. El usuario nuevo **deberá cambiarla** en su primer ingreso.
- **Editar** (doble clic en la fila): cambiar nombre, login, rol o estado. La contraseña no se
  edita aquí.
- **Activar / Desactivar**: un usuario inactivo no puede iniciar sesión, pero se conserva (con
  su historial).
- **Resetear contraseña**: define una contraseña temporal para alguien que la olvidó; esa
  persona deberá cambiarla en su próximo ingreso.

La columna **Cambio pendiente** muestra quién tiene un cambio de contraseña obligatorio sin hacer.

**Protecciones:** el sistema no te deja **desactivar ni quitar el rol al último administrador
activo**, ni **desactivar tu propio usuario** (para que nadie se quede sin acceso al sistema).

---

## 10. Respaldos

El sistema crea un **respaldo automático** de la base de datos **una vez al día**, en la
carpeta `Backups/` junto al programa (nombre con fecha y hora). Conserva los **15 respaldos**
más recientes y borra los más antiguos. El respaldo se hace en segundo plano y nunca impide
que el sistema arranque.

### Módulo Respaldos *(solo administrador)*

En el menú lateral, **💾 Respaldos** (**Ctrl + 7**) permite:

- **Respaldar ahora**: crea una copia al instante.
- **Restaurar**: reemplaza la base actual por la de un respaldo (de la lista, o **desde un
  archivo** como un pendrive). Pide confirmación, guarda una copia del estado actual (`.previo`)
  y **reinicia** la aplicación. *Disponible solo con SQLite.*
- **Abrir carpeta**: abre la carpeta de respaldos en el explorador.

> **Copia externa (recomendado):** si se configura una **carpeta externa** (red, USB o nube) en
> `App.config` (`CarpetaRespaldoExterno`), cada respaldo se copia también ahí, para sobrevivir a
> una falla del disco. Con **SQL Server** los respaldos se gestionan en el servidor (ver la guía
> de despliegue).

---

## 11. Atajos de teclado

**Navegación (en cualquier pantalla)**

| Tecla | Acción |
|---|---|
| Ctrl + 1 … 7 | Inicio / Ventas / Productos / Caja / Reportes / Usuarios / Respaldos *(últimos tres solo admin)* |

**Ventas:** F2 buscar · F3 escáner · F4 descuento · F6 nueva venta · Esc cancelar · F12 cobrar
**Productos:** F2 buscar · F5 recargar · Esc limpiar formulario
**Caja / Reportes / Inicio:** F5 refrescar

---

## 12. Preguntas frecuentes

**No me aparece el módulo Reportes / Usuarios / no puedo abrir la caja.**
Esas acciones son solo para administradores. Inicia sesión con una cuenta de administrador.

**Olvidé mi contraseña.**
Pídele a un administrador que la **resetee** desde *Usuarios*: te dará una temporal y el sistema
te pedirá cambiarla al entrar.

**Quiero vender un producto pero no aparece.**
Puede estar **inactivo** o sin stock. Búscalo en Productos; el administrador puede activarlo o
reponer stock.

**El sistema no me deja eliminar un producto.**
Tiene ventas registradas. Usa **Desactivar** en vez de Eliminar para conservar el historial.

**Apliqué un descuento y el total quedó en $0.**
El descuento al total no puede superar el subtotal. Si quitaste productos después de
aplicarlo, el descuento se ajusta al nuevo subtotal automáticamente.

**¿Dónde quedan los datos?**
En `pos.db` junto al ejecutable, y los respaldos en `Backups/`. No se necesita internet.

**Cerré una venta por error / quiero anularla.**
Un administrador puede **anular** la venta desde *Reportes* (devuelve el stock).

---

Para detalles técnicos del sistema, ver [ARQUITECTURA.md](ARQUITECTURA.md) y
[MODELO-DATOS.md](MODELO-DATOS.md).
