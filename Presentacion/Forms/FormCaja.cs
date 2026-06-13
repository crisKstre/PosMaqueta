using System;
using System.Windows.Forms;
using Dominio.Eventos;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public partial class FormCaja : Form
    {
        private readonly CajaService cajaService = new CajaService();

        public FormCaja()
        {
            InitializeComponent();
        }

        private void FormCaja_Load(object sender, EventArgs e)
        {
            NotificadorCambios.Cambio += OnCambioDatos;
            RefrescarEstado();
        }

        private void FormCaja_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotificadorCambios.Cambio -= OnCambioDatos;
        }

        private void OnCambioDatos(string entidad)
        {
            // Las ventas cambian el resumen del turno; la caja cambia el estado
            if (entidad == Entidad.Caja || entidad == Entidad.Venta)
                RefrescarEstado();
        }

        private void RefrescarEstado()
        {
            var caja = cajaService.ObtenerCajaAbierta();
            bool esAdmin = Sesion.EsAdmin;

            if (caja == null)
            {
                // Estado: sin turno abierto
                pnlAbierta.Visible = false;
                pnlAbrir.Visible = true;
                lblTituloEstado.Text = "Caja — sin turno abierto";

                lblPermisoAbrir.Visible = !esAdmin;
                txtMontoInicial.Enabled = esAdmin;
                btnAbrir.Enabled = esAdmin;
            }
            else
            {
                // Estado: turno abierto
                pnlAbrir.Visible = false;
                pnlAbierta.Visible = true;
                lblTituloEstado.Text = "Caja — turno abierto";

                var resumen = cajaService.ObtenerResumen(caja.IdCaja);
                decimal efectivoEsperado = cajaService.CalcularEfectivoEsperado(caja, resumen);

                lblInfoApertura.Text = "Abierta el " + caja.FechaApertura.ToString("dd/MM/yyyy HH:mm") +
                                       "   ·   Monto inicial: $" + caja.MontoInicial.ToString("N0");

                lblVentasVal.Text = resumen.CantidadVentas.ToString();
                lblTotalVal.Text = "$" + resumen.TotalVendido.ToString("N0");
                lblEfectivoVal.Text = "$" + efectivoEsperado.ToString("N0");

                lblDesgloseVal.Text =
                    "Efectivo: $" + resumen.TotalEfectivo.ToString("N0") +
                    "    Tarjeta: $" + resumen.TotalTarjeta.ToString("N0") +
                    "    Transferencia: $" + resumen.TotalTransferencia.ToString("N0");

                lblPermisoCerrar.Visible = !esAdmin;
                txtMontoReal.Enabled = esAdmin;
                btnCerrar.Enabled = esAdmin;
            }

            lblMensaje.Visible = false;
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMontoInicial.Text, out decimal monto) || monto < 0)
            {
                MostrarMensaje("Ingresa un monto inicial válido.");
                return;
            }

            try
            {
                cajaService.AbrirCaja(Sesion.UsuarioActual.IdUsuario, monto);
                txtMontoInicial.Clear();
                RefrescarEstado();
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMontoReal.Text, out decimal montoReal) || montoReal < 0)
            {
                MostrarMensaje("Ingresa el monto real contado.");
                return;
            }

            var confirmar = MessageBox.Show(
                "¿Cerrar la caja con un monto contado de $" + montoReal.ToString("N0") + "?",
                "Cerrar caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmar != DialogResult.Yes) return;

            try
            {
                decimal diferencia = cajaService.CerrarCaja(montoReal);
                MostrarResultadoArqueo(diferencia);
                txtMontoReal.Clear();
                RefrescarEstado();
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message);
            }
        }

        private void MostrarResultadoArqueo(decimal diferencia)
        {
            string texto;
            if (diferencia == 0)
                texto = "Caja cerrada. Arqueo cuadrado exactamente.";
            else if (diferencia > 0)
                texto = "Caja cerrada. SOBRANTE de $" + diferencia.ToString("N0") +
                        " (hay más efectivo del esperado).";
            else
                texto = "Caja cerrada. FALTANTE de $" + Math.Abs(diferencia).ToString("N0") +
                        " (falta efectivo respecto al esperado).";

            MessageBox.Show(texto, "Resultado del arqueo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MostrarMensaje(string msg)
        {
            lblMensaje.Text = msg;
            lblMensaje.Visible = true;
        }
    }
}
