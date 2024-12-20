﻿using PetGraphBackend.Configs;
using PetGraphBackend.Objetos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PetGraph.Views
{
    public partial class GraphForm : Form
    {
        private string formula;
        private bool teclaPresionadaR = false, teclaPresionadaL = false,
            teclaPresionadaU = false, teclaPresionadaD = false, 
            teclaPresionadaEnter = false, scrollAndando = false;
        private int playerX = 5, playerY = 4; // hasta 9 que es longitud de vector
        private int numeroLabel = 0, zoomX = 0, zoomY = 0, puntaje = 0;
        List<PointGraph> listadoPuntos = new List<PointGraph>();
        private Stopwatch temporizador = new Stopwatch();
        private Timer controladorTemp = new Timer();

        public GraphForm(Player player)
        {
            InitializeComponent();
            ReproductorSonidos.ReproducirMusica("graph-theme.mp3");
            pictureBox1.Image = player.imgAnimal;
            Text = $"PetGraph - ¡Juega con {player.namePlayer}!";
            label1.Text = $"Puntaje: {puntaje}";
            label2.Text = $"({obtenerNumerosX()[playerX]}, {obtenerNumerosY()[playerY]})";  

            if (Screen.PrimaryScreen.WorkingArea.Width >= 1900
                && Screen.PrimaryScreen.WorkingArea.Height >= 1040) Size = new Size(917, 755);
            else if (Screen.PrimaryScreen.WorkingArea.Width >= 1360
                && Screen.PrimaryScreen.WorkingArea.Height >= 700) Size = new Size(917, 730);

            if (Screen.PrimaryScreen.WorkingArea.Width <= 1900
                && Screen.PrimaryScreen.WorkingArea.Height <= 1040)
            {
                MouseWheel += GraphForm_Wheel;
                controladorTemp.Interval = 250;
                controladorTemp.Tick += (s, e) =>
                {
                    if (temporizador.ElapsedMilliseconds > 500) scrollAndando = false;
                };
                controladorTemp.Start();
            }
        }

        private void GraphForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Width: 900; Height: 750 (FORMULARIO)
            g.TranslateTransform(0, -VerticalScroll.Value);

            // Lapiz color y trazo
            Pen pen = new Pen(ConfiguracionTemas.ObtenerColorParaGrafica(), 3);

            // Eje de Ordenadas
            int Ox1 = 445;
            int Oy1 = 20;
            int Oy2 = 690;
            Point startPoint = new Point(Ox1, Oy1);
            Point endPoint = new Point(Ox1, Oy2);
            g.DrawLine(pen, startPoint, endPoint);

            // Puntas FLECHAS
            Point[] puntaSuperior = { new Point(Ox1, Oy1 - 2),
                new Point(Ox1 - 10, Oy1 + 15), new Point(Ox1 + 10, Oy1 + 15) };
            g.FillPolygon(new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()), puntaSuperior);

            Point[] puntaInferior = { new Point(Ox1, Oy2 + 2),
                new Point(Ox1 - 10, Oy2 - 15), new Point(Ox1 + 10, Oy2 - 15) };
            g.FillPolygon(new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()), puntaInferior);

            // Eje de Abscisas
            int Ax1 = 40;
            int Ax2 = 850;
            int Ay1 = 355;
            Point startPoint2 = new Point(Ax1, Ay1);
            Point endPoint2 = new Point(Ax2, Ay1);
            g.DrawLine(pen, startPoint2, endPoint2);
            g.DrawLine(pen, startPoint2, endPoint2);

            // Puntas FLECHAS
            Point[] puntaDerecha = { new Point(Ax2 + 2, Ay1),
                new Point(Ax2 - 15, Ay1 - 10), new Point(Ax2 - 15, Ay1 + 10) };
            g.FillPolygon(new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()), puntaDerecha);

            Point[] puntaIzquierda = { new Point(Ax1 - 2, Ay1),
                new Point(Ax1 + 15, Ay1 - 10), new Point(Ax1 + 15, Ay1 + 10) };
            g.FillPolygon(new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()), puntaIzquierda);

            // DIBUJAR LINEAS Y NÚMEROS EN LOS EJES DONDE SE COLOCARAN PUNTOS
            int inicioX = 70;
            double[] numeroXPositivos = obtenerNumerosX().Where(n => n > 0).ToArray();
            double[] numeroXNegativos = obtenerNumerosX().Where(n => n < 0).ToArray();

            for (int lineas = 0; lineas < 5; lineas++)
            {
                g.DrawLine(pen, new Point(Ox1 - inicioX, 345), new Point(Ox1 - inicioX, 365));
                g.DrawString(numeroXNegativos[lineas].ToString().Replace(",", "."), new Font("Bahnschrift Condensed", 15),
                    new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()),
                    numeroXNegativos[lineas] > -1 ?
                    new Point(Ox1 - (70 * (5 - lineas)) - 18, 375)
                    : new Point(numeroXNegativos[lineas] < -9 ? Ox1 - (70 * (5 - lineas)) - 16 :
                    Ox1 - (70 * (5 - lineas)) - 14, 375));

                g.DrawLine(pen, new Point(Ox1 + inicioX, 345), new Point(Ox1 + inicioX, 365));
                g.DrawString(numeroXPositivos[lineas].ToString().Replace(",", "."), new Font("Bahnschrift Condensed", 15),
                    new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()),
                    numeroXPositivos[lineas] < 1 ?
                    new Point(Ox1 + inicioX - 12, 375)
                    : new Point(numeroXPositivos[lineas] > 9 ? Ox1 + inicioX - 9 : Ox1 + inicioX - 6, 375));
                inicioX += 70;
            }

            int inicioY = 70;
            double[] numeroYPositivos = obtenerNumerosY().Where(n => n > 0).ToArray();
            double[] numeroYNegativos = obtenerNumerosY().Where(n => n < 0).ToArray();
            for (int lineas = 0; lineas < 4; lineas++)
            {
                g.DrawLine(pen, new Point(Ox1 - 13, Ay1 - inicioY), new Point(Ox1 + 13, Ay1 - inicioY));
                g.DrawString(numeroYPositivos[lineas].ToString().Replace(",", "."), new Font("Bahnschrift Condensed", 15),
                    new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()),
                    numeroYPositivos[lineas] > 0.9
                    ? new Point(numeroYPositivos[lineas] > 9 ? Ox1 - 35 : Ox1 - 30, Ay1 - inicioY - 10)
                    : new Point(Ox1 - 38, Ay1 - inicioY - 10));

                g.DrawLine(pen, new Point(Ox1 - 13, Ay1 + inicioY), new Point(Ox1 + 13, Ay1 + inicioY));
                g.DrawString(numeroYNegativos[lineas].ToString().Replace(",", "."), new Font("Bahnschrift Condensed", 15),
                    new SolidBrush(ConfiguracionTemas.ObtenerColorParaGrafica()),
                    numeroYNegativos[lineas] < -0.9 ?
                    new Point(numeroYNegativos[lineas] < -9
                    ? Ox1 - 43 : Ox1 - 38, Ay1 + (70 * (4 - lineas)) - 10) :
                    new Point(Ox1 - 46, Ay1 + (70 * (4 - lineas)) - 10));
                inicioY += 70;
            }

            pen.Dispose();
        }

        private void RandomizarPuntos()
        {
            Random randomizador = new Random();
            int cantPuntos = randomizador.Next(1, 6);
            List<string> puntos = new List<string>();

            for (int i = 0; i < cantPuntos; i++)
            {
                int x = randomizador.Next(-10, 10);
                int y = randomizador.Next(-10, 10);
                puntos.Add($"({x}; {y})");
            }

            formula = string.Join(",  ", puntos);
        }

        private void RandomizarFuncion()
        {
            Random randomizador = new Random();
            int cantTerminos = randomizador.Next(1, 3);
            formula = "";

            if (cantTerminos == 1) // X o Y
            {
                formula = randomizador.Next(1, 3) == 1 ? "x = " : "y = ";
                formula += $"{randomizador.Next(-10, 10)}";
            }
            else if (cantTerminos == 2) // X +- Y =
            {
                do
                {
                    formula = randomizador.Next(1, 3) == 1 ? $"{randomizador.Next(-10, 10)}x" : "x";
                    formula += randomizador.Next(1, 3) == 1 ? " + " : " - ";
                    formula += randomizador.Next(1, 3) == 1 ? $"{randomizador.Next(-10, 10)}y" : "y";
                } while (formula == "x - y" || formula == "x + y");
                formula += $" = {randomizador.Next(-10, 10)}";
            }
            else if (cantTerminos == 3) // X +- N = Y || Y +- N = X
            {
                formula = randomizador.Next(1, 3) == 1 ? "x" : "y";
                if (formula == "x")
                {
                    formula = randomizador.Next(1, 3) == 1 ? $"{randomizador.Next(-10, 10)}x" : "x";
                    formula += randomizador.Next(1, 3) == 1
                    ? $" + {randomizador.Next(-10, 10)} = " : $" - {randomizador.Next(-10, 10)} = ";
                    formula += randomizador.Next(1, 3) == 1 ? $"{randomizador.Next(-10, 10)}y" : "y";
                }
                else
                {
                    formula = randomizador.Next(1, 3) == 1 ? $"{randomizador.Next(-10, 10)}y" : "y";
                    formula += randomizador.Next(1, 3) == 1
                    ? $" + {randomizador.Next(-10, 10)} = " : $" - {randomizador.Next(-10, 10)} = ";
                    formula += randomizador.Next(1, 3) == 1 ? $"{randomizador.Next(-10, 10)}x" : "x";
                }
            }

            MessageBox.Show(formula);
        }

        private void labelCerrar_Click(object sender, EventArgs e)
        {
            ReproductorSonidos.ReproducirSonido("menu-select.mp3");
            panel2.Visible = false;
        }

        public double[] obtenerNumerosX()
        {
            if (zoomX == 1) return new double[] { -10, -9, -8, -7, -6, 0, 6, 7, 8, 9, 10 };
            else if (zoomX == 2) return new double[] { -15, -14, -13, -12, -11, 0, 11, 12, 13, 14, 15 };
            else if (zoomX == 3) return new double[] { -20, -19, -18, -17, -16, 0, 16, 17, 18, 19, 20 };
            else if (zoomX == -1) return new double[] { -0.9, -0.8, -0.7, -0.6, -0.5, 0, 0.5, 0.6, 0.7, 0.8, 0.9 };
            else if (zoomX == -2) return new double[] { -0.5, -0.4, -0.3, -0.2, -0.1, 0, 0.1, 0.2, 0.3, 0.4, 0.5 };
            else return new double[] { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 };
        }

        public double[] obtenerNumerosY()
        {
            if (zoomY == 1) return new double[] { -8, -7, -6, -5, 0, 5, 6, 7, 8 };
            else if (zoomY == 2) return new double[] { -12, -11, -10, -9, 0, 9, 10, 11, 12 };
            else if (zoomY == 3) return new double[] { -16, -15, -14, -13, 0, 13, 14, 15, 16 };
            else if (zoomY == -1) return new double[] { -0.9, -0.8, -0.7, -0.6, 0, 0.6, 0.7, 0.8, 0.9 };
            else if (zoomY == -2) return new double[] { -0.5, -0.4, -0.3, -0.2, 0, 0.2, 0.3, 0.4, 0.5 };
            else if (zoomY == -3) return new double[] { -0.4, -0.3, -0.2, -0.1, 0, 0.1, 0.2, 0.3, 0.4 };
            else return new double[] { -4, -3, -2, -1, 0, 1, 2, 3, 4 };
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            ReproductorSonidos.ReproducirSonido("menu-select.mp3");
            AlertaControles.Show();
        }

        private void GraphForm_Scroll(object sender, ScrollEventArgs e)
        {
            scrollAndando = true;
            temporizador.Restart();
        }

        private void GraphForm_Wheel(object sender, MouseEventArgs e)
        {
            scrollAndando = true;
            temporizador.Restart();
        }

        private void GraphForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!scrollAndando)
            {
                if (!teclaPresionadaL && !teclaPresionadaR && !teclaPresionadaU
                && !teclaPresionadaD && !teclaPresionadaEnter)
                {
                    if (panel1.Location.X - AutoScrollPosition.X < 761)
                    {
                        if (e.KeyCode == Keys.Right)
                        {
                            ReproductorSonidos.ReproducirSonido("menu-move.mp3");
                            panel1.Location = new Point(panel1.Location.X + 70, panel1.Location.Y);
                            teclaPresionadaR = true;
                            playerX++;
                            label2.Text = $"({obtenerNumerosX()[playerX].ToString().Replace(",", ".")}; " +
                                $"{obtenerNumerosY()[playerY].ToString().Replace(",", ".")})";
                        }
                    }

                    if (panel1.Location.X - AutoScrollPosition.X > 61)
                    {
                        if (e.KeyCode == Keys.Left)
                        {
                            ReproductorSonidos.ReproducirSonido("menu-move.mp3");
                            panel1.Location = new Point(panel1.Location.X - 70, panel1.Location.Y);
                            teclaPresionadaL = true;
                            playerX--;
                            label2.Text = $"({obtenerNumerosX()[playerX].ToString().Replace(",", ".")}; " +
                                $"{obtenerNumerosY()[playerY].ToString().Replace(",", ".")})";
                        }
                    }

                    if (panel1.Location.Y - AutoScrollPosition.Y > 39)
                    {
                        if (e.KeyCode == Keys.Up)
                        {
                            ReproductorSonidos.ReproducirSonido("menu-move.mp3");
                            panel1.Location = new Point(panel1.Location.X, panel1.Location.Y - 70);
                            teclaPresionadaD = true;
                            playerY++;
                            label2.Text = $"({obtenerNumerosX()[playerX].ToString().Replace(",", ".")}; " +
                                $"{obtenerNumerosY()[playerY].ToString().Replace(",", ".")})";
                        }
                    }

                    if (panel1.Location.Y - AutoScrollPosition.Y < 599)
                    {
                        if (e.KeyCode == Keys.Down)
                        {
                            ReproductorSonidos.ReproducirSonido("menu-move.mp3");
                            panel1.Location = new Point(panel1.Location.X, panel1.Location.Y + 70);
                            teclaPresionadaU = true;
                            playerY--;
                            label2.Text = $"({obtenerNumerosX()[playerX].ToString().Replace(",", ".")}; " +
                                $"{obtenerNumerosY()[playerY].ToString().Replace(",", ".")})";
                        }
                    }

                    if (e.KeyCode == Keys.Enter)
                    {
                        ReproductorSonidos.ReproducirSonido("put-point.mp3");
                        bool crearLabel = true;

                        foreach (Control control in Controls)
                        {
                            if (control is Label)
                            {
                                if (control.Name.Contains("labelPunto")
                                    && panel1.Bounds.IntersectsWith(control.Bounds))
                                {
                                    Controls.Remove(control);
                                    listadoPuntos.RemoveAll(point => point.contenido == control);
                                    crearLabel = false;
                                    break;
                                }
                            }
                        }

                        if (crearLabel)
                        {
                            Label labelPunto = new Label
                            {
                                Name = $"labelPunto{numeroLabel}",
                                Text = "◉\n",
                                Font = obtenerNumerosX()[playerX].ToString().Contains(",")
                                || obtenerNumerosY()[playerY].ToString().Contains(",") ?
                                new Font("Bahnschrift Condensed", 11f) : new Font("Bahnschrift Condensed", 14f), //11 cuando , y 13 
                                Location = obtenerNumerosX()[playerX].ToString().Contains(",")
                                || obtenerNumerosY()[playerY].ToString().Contains(",") ?
                                new Point(panel1.Location.X + 6, panel1.Location.Y + 21) :
                                new Point(panel1.Location.X + 6, panel1.Location.Y + 25),
                                BackColor = Color.Transparent,
                                ForeColor = label2.ForeColor,
                                TextAlign = ContentAlignment.MiddleCenter,
                                AutoSize = false,
                                Size = new Size(60, label2.Size.Height + 17)
                            };
                            numeroLabel++;

                            labelPunto.Text += label2.Text;

                            Controls.Add(labelPunto);
                            listadoPuntos.Add(new
                                PointGraph(obtenerNumerosX()[playerX], obtenerNumerosY()[playerY], labelPunto));
                        }

                        teclaPresionadaEnter = true;
                    }
                }

                if (e.Control)
                {
                    if (e.KeyCode == Keys.Z)
                    {
                        zoomX = zoomX == 3 ? 0 : zoomX + 1;
                        cargarPuntos();
                    }
                    else if (e.KeyCode == Keys.C)
                    {
                        zoomY = zoomY == 3 ? 0 : zoomY + 1;
                        cargarPuntos();
                    }
                    else if (e.KeyCode == Keys.X)
                    {
                        zoomX = zoomX == -2 ? 0 : zoomX - 1;
                        cargarPuntos();
                    }
                    else if (e.KeyCode == Keys.V)
                    {
                        zoomY = zoomY == -3 ? 0 : zoomY - 1;
                        cargarPuntos();
                    }
                }
            }
        } 

        private void GraphForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right) teclaPresionadaR = false;
            else if (e.KeyCode == Keys.Left) teclaPresionadaL = false;
            else if (e.KeyCode == Keys.Up) teclaPresionadaD = false;
            else if (e.KeyCode == Keys.Down) teclaPresionadaU = false;
            else if (e.KeyCode == Keys.Enter) teclaPresionadaEnter = false;
        }

        private void cargarPuntos()
        {
            label2.Text = $"({obtenerNumerosX()[playerX].ToString().Replace(",", ".")}; " +
                $"{obtenerNumerosY()[playerY].ToString().Replace(",", ".")})";

            List<Label> labelsAEliminar = Controls.OfType<Label>()
                .Where(label => label.Name.Contains("labelPunto")).ToList();

            List<PointGraph> labelsAAnadir = listadoPuntos.OfType<PointGraph>()
                .Where(punto => obtenerNumerosX().Contains(punto.valorX)
                && obtenerNumerosY().Contains(punto.valorY)).ToList();

            // Acomodar label para decimales estaticos
            foreach (PointGraph labelEditar in labelsAAnadir)
            {
                if (zoomX == -1)
                {
                    if (labelEditar.valorX == -0.5)
                    {
                        labelEditar.contenido.Location =
                            new Point(375 - 27, labelEditar.contenido.Location.Y);
                    }
                    else if (labelEditar.valorX == 0.5)
                    {
                        labelEditar.contenido.Location =
                    new Point(445 + 70 - 27, labelEditar.contenido.Location.Y);
                    }
                }
                else if (zoomX == -2)
                {
                    if (labelEditar.valorX == -0.5)
                    {
                        labelEditar.contenido.Location =
                            new Point(95 - 27, labelEditar.contenido.Location.Y);
                    }
                    else if (labelEditar.valorX == 0.5)
                    {
                        labelEditar.contenido.Location =
                    new Point(445 + (70 * 5) - 27, labelEditar.contenido.Location.Y);
                    }
                }

                if (zoomY == -2)
                {
                    if (labelEditar.valorY == -0.4)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 + (70 * (4 - 1)) - 14);
                    }
                    else if (labelEditar.valorY == -0.3)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 + (70 * (4 - 2)) - 14);
                    }
                    else if (labelEditar.valorY == -0.2)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 + (70 * (4 - 3)) - 14);
                    }
                    else if (labelEditar.valorY == 0.4)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 - 210 - 13);
                    }
                    else if (labelEditar.valorY == 0.3)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 - 140 - 13);
                    }
                    else if (labelEditar.valorY == 0.2)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 - 70 - 13);
                    }
                }
                else if (zoomY == -3)
                {
                    if (labelEditar.valorY == -0.4)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 + (70 * 4) - 14);
                    }
                    else if (labelEditar.valorY == -0.3)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 + (70 * (4 - 1)) - 14);
                    }
                    else if (labelEditar.valorY == -0.2)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 + (70 * (4 - 2)) - 14);
                    }
                    else if (labelEditar.valorY == 0.4)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 - 280 - 13);
                    }
                    else if (labelEditar.valorY == 0.3)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 - 210 - 13);
                    }
                    else if (labelEditar.valorY == 0.2)
                    {
                        labelEditar.contenido.Location =
                            new Point(labelEditar.contenido.Location.X, 355 - 140 - 13);
                    }
                }
            }

            foreach (Label labelEliminar in labelsAEliminar) Controls.Remove(labelEliminar);
            foreach (PointGraph pointGraph in labelsAAnadir) Controls.Add(pointGraph.contenido);

            label2.Font = obtenerNumerosX()[playerX].ToString().Contains(",") &&
                              obtenerNumerosY()[playerY].ToString().Contains(",") ?
                              new Font("Bahnschrift Condensed", 11f) : new Font("Bahnschrift Condensed", 14f);
            Invalidate();
        }

        private void MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
            ReproductorSonidos.ReproducirSonido("menu-move.mp3");
        }

        private void buttonCompletado_Click(object sender, EventArgs e)
        {
            string puntos = string.Join(", ",
                listadoPuntos.Select(punto => $"{punto.contenido.Text.Split('\n')[1]}"));
            int eleccion = AlertaVerificacion.Show(puntos);

            if (eleccion == 1)
            {
                if (formula.Contains(";"))
                {
                    string[] puntosFormula = formula.Replace("{", "").Replace("}", "").Split(',');
                    string[] puntosSeleccionados = puntos.Split(',');

                    if (puntosFormula.Length == puntosSeleccionados.Length)
                    {
                        bool correcto = true;

                        foreach (string puntoF in puntosFormula)
                        {
                            bool encontrado = false;

                            foreach (string puntoS in puntosSeleccionados)
                            {

                                string puntoFComparar = puntoF.Replace("(", "").Replace(")", "").Trim();
                                string puntoSComparar = puntoS.Replace("(", "").Replace(")", "").Trim();

                                if (puntoFComparar == puntoSComparar)
                                {
                                    encontrado = true;
                                    break;
                                }
                            }

                            if (!encontrado)
                            {
                                correcto = false;
                                break;
                            }
                        }

                        if (correcto)
                        {
                            Alerta.Show("¡Muy bien! Has puesto todos los puntos correctamente, felicidades." +
                                "\n¡Sigue asi y ve a por más todavía!",
                                "¡Gracias!", "correcto");
                            puntaje++;
                            ComenzarDeNuevo();
                        }
                        else
                        {
                            Alerta.Show("Oh, lastima... Has puesto uno/varios puntos incorrectos." +
                                "\n¡Presta más atención la próxima, ánimo no te desanimes que sé " +
                                "\nque puedes hacerlo mejor!",
                                "¡Me esforzaré!", "erroneo");

                            if (puntaje == 0)
                            {
                                Alerta.Show("Oh, lastima... Tus puntaje ha decendido a menos que 0." +
                                "\n¡Esfuerzate más la próxima, recuerda, tus límites los estableces " +
                                "\ntú y nadie más, ÁNIMO!",
                                "Salir", "derrota");
                                Close();
                            }
                            else
                            {
                                puntaje--;
                                label1.Text = $"Puntaje: {puntaje}";
                            }
                        }
                    }
                    else
                    {
                        Alerta.Show("¡Hey velocista, espera! Te han faltado o te has pasado de puntos." +
                                "\n¡Presta más atención la próxima, ánimo no te desanimes que sé " +
                                "\nque puedes hacerlo mejor!",
                                "¡Me esforzaré!", "erroneo");

                        if (puntaje == 0)
                        {
                            Alerta.Show("Oh, lastima... Tus puntaje ha decendido a menos que 0." +
                                "\n¡Esfuerzate más la próxima, recuerda, tus límites los estableces " +
                                "\ntú y nadie más, ÁNIMO!",
                                "Salir", "derrota");
                            Close();
                        }
                        else
                        {
                            puntaje--;
                            label1.Text = $"Puntaje: {puntaje}";
                        }
                    }
                }
                else if (formula.Contains("x") || formula.Contains("y"))
                {

                }
            }

            this.ActiveControl = null;
        }

        private void ClickObjetivo(object sender, EventArgs e)
        {
            // RandomizarFuncion();
            if (formula == null) RandomizarPuntos();
            ReproductorSonidos.ReproducirSonido("menu-select.mp3");
            panel2.Visible = true;
            label4.Text = $"Represente el siguiente conjunto o función en el eje de cordenadas cartesianas: " +
                "\n\n{" + $"{formula}" + "}";
        }

        private void ComenzarDeNuevo()
        {
            label1.Text = $"Puntaje: {puntaje}";
            listadoPuntos.Clear();
            zoomX = 0;
            zoomY = 0;
            cargarPuntos();

            RandomizarPuntos();
            label4.Text = $"Represente el siguiente conjunto o función en el eje de cordenadas cartesianas: " +
            "\n\n{" + $"{formula}" + "}";
            //if (puntaje < 6)
            //{

            //}
            //else
            //{
            //    if (new Random().Next(1, 3) == 1) RandomizarPuntos();
            //    else RandomizarFuncion();
            //}
        }

        private void ResolverEcuacion()
        {
            double OrdenadaAlOrigen = 0;

            // Verificar cuantos terminos tiene
            // 3 TÉRMINOS
            if (formula.Contains("+") || formula.Contains("-") && formula.Contains("="))
            {   // X +- Y = N || X +- N = Y || Y +- N = X
                string[] terminos = Regex.Split(formula, @"(?=[+-=])|(?<=[+-=])");

                if (terminos[0].Contains("x"))
                {
                    // terminos[0].Any(char.IsDigit)
                }
                else
                {
                    
                }

            } // 1 TERMINO
            else
            {
                string puntoRecta;
                puntoRecta = formula.Contains("x =") 
                    ? $"({formula.Split('=')[1].Trim()}; 0)" : $"(0; {formula.Split('=')[1].Trim()})";
            }
        }
    }
}
