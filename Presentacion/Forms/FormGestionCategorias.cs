using System;
using System.Windows.Forms;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    public class FormGestionCategorias : Form
    {
        private readonly CategoriaService service;
        private ListBox   lstCategorias;
        private TextBox   txtNueva;
        private Button    btnAgregar;
        private Button    btnEliminar;
        private Label     lblMensaje;

        public FormGestionCategorias(CategoriaService svc)
        {
            service = svc;
            InitUI();
            CargarLista();
        }

        private void InitUI()
        {
            this.Text = "Gestionar Categorías";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new System.Drawing.Size(420, 480);
            this.BackColor = System.Drawing.Color.White;

            var lblTitulo = new Label { Text = "Categorías", Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(15,15,17), AutoSize = true, Location = new System.Drawing.Point(20, 18) };

            lstCategorias = new ListBox { Font = new System.Drawing.Font("Segoe UI", 12F), Location = new System.Drawing.Point(20, 58),
                Size = new System.Drawing.Size(370, 240), BorderStyle = BorderStyle.FixedSingle,
                ForeColor = System.Drawing.Color.FromArgb(15,15,17) };

            var lblNueva = new Label { Text = "Nueva categoría:", Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = System.Drawing.Color.FromArgb(60,60,65), AutoSize = true, Location = new System.Drawing.Point(20, 318) };

            txtNueva = new TextBox { Font = new System.Drawing.Font("Segoe UI", 12F), Location = new System.Drawing.Point(20, 340),
                Size = new System.Drawing.Size(260, 36), BorderStyle = BorderStyle.FixedSingle };

            btnAgregar = new Button { Text = "Agregar", Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(15,15,17), ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Location = new System.Drawing.Point(290, 338), Size = new System.Drawing.Size(100, 38) };
            btnAgregar.FlatAppearance.BorderSize = 0;

            btnEliminar = new Button { Text = "Eliminar seleccionada", Font = new System.Drawing.Font("Segoe UI", 10F),
                BackColor = System.Drawing.Color.White, ForeColor = System.Drawing.Color.FromArgb(180,30,30),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Location = new System.Drawing.Point(20, 392), Size = new System.Drawing.Size(220, 36) };
            btnEliminar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200,180,180);

            lblMensaje = new Label { AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9.5F),
                ForeColor = System.Drawing.Color.FromArgb(180,30,30), Location = new System.Drawing.Point(20, 436), Visible = false };

            btnAgregar.Click  += (s, e) => AgregarCategoria();
            btnEliminar.Click += (s, e) => EliminarCategoria();
            txtNueva.KeyDown  += (s, e) => { if (e.KeyCode == Keys.Enter) AgregarCategoria(); };

            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblTitulo, lstCategorias, lblNueva, txtNueva, btnAgregar, btnEliminar, lblMensaje });
        }

        private void CargarLista()
        {
            lstCategorias.Items.Clear();
            foreach (var c in service.ObtenerTodas())
                lstCategorias.Items.Add(c);
            lstCategorias.DisplayMember = "Nombre";
        }

        private void AgregarCategoria()
        {
            string nombre = txtNueva.Text.Trim();
            if (string.IsNullOrEmpty(nombre)) return;
            try
            {
                service.Agregar(nombre);
                txtNueva.Clear();
                lblMensaje.Visible = false;
                CargarLista();
            }
            catch (Exception ex) { lblMensaje.Text = ex.Message; lblMensaje.Visible = true; }
        }

        private void EliminarCategoria()
        {
            if (lstCategorias.SelectedItem == null) { lblMensaje.Text = "Selecciona una categoría."; lblMensaje.Visible = true; return; }
            var cat = (Categoria)lstCategorias.SelectedItem;
            if (MessageBox.Show("¿Eliminar la categoría \"" + cat.Nombre + "\"?", "Eliminar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                service.Eliminar(cat.IdCategoria);
                lblMensaje.Visible = false;
                CargarLista();
            }
            catch (Exception ex) { lblMensaje.Text = ex.Message; lblMensaje.Visible = true; }
        }
    }
}
