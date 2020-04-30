﻿using Eto.Drawing;
using Eto.Forms;
using DWSIM.DynamicsManager;
using DWSIM.ExtensionMethods;
using DWSIM.UI.Shared;
using ext = DWSIM.UI.Shared.Common;
using System.CodeDom;
using System.Linq;
using System.Collections.Generic;
using System;
using DWSIM.Interfaces;

namespace DWSIM.UI.Desktop.Editors.Dynamics
{

    public class DynamicsManagerControl : TableLayout
    {
        string imgprefix = "DWSIM.UI.Desktop.Editors.Resources.Icons.";

        public CheckBox chkDynamics;

        private Panel eventEditor, ceiEditor, mvEditor, schEditor;

        private ListBox lbEventSets, lbEvents, lbCEM, lbCEI, lbSchedules, lbIntegrators, lbVariables;

        private Shared.Flowsheet Flowsheet;

        public DynamicsManagerControl(Shared.Flowsheet fs) : base()
        {
            Flowsheet = fs;
        }

        public void Init()
        {

            var tl1 = new TableLayout { Padding = new Padding(5), Spacing = new Size(10, 10) };

            var tr1 = new TableRow();

            chkDynamics = new CheckBox { Text = "Dynamic Mode Enabled" };

            tr1.Cells.Add(chkDynamics);

            tr1.Cells.Add(null);

            tl1.Rows.Add(tr1);
             
            Rows.Add(new TableRow(tl1));

            var DocumentContainer = new DocumentControl() { AllowReordering = false, DisplayArrows = false };

            DocumentContainer.Pages.Add(new DocumentPage { Text = "Model Status", Closable = false });
            DocumentContainer.Pages.Add(new DocumentPage { Text = "Event Sets", Closable = false });
            DocumentContainer.Pages.Add(new DocumentPage { Text = "Cause-and-Effect Matrices", Closable = false });
            DocumentContainer.Pages.Add(new DocumentPage { Text = "Integrators", Closable = false });
            DocumentContainer.Pages.Add(new DocumentPage { Text = "Schedules", Closable = false });
            //DocumentContainer.Pages.Add(new DocumentPage { Text = "Controllers", Closable = false });
            //DocumentContainer.Pages.Add(new DocumentPage { Text = "Indicators", Closable = false });

            var tl2 = new TableLayout { Padding = new Padding(5), Spacing = new Size(10, 10) };

            var tr2 = new TableRow();

            tr2.Cells.Add(DocumentContainer);

            tl2.Rows.Add(tr2);

            Rows.Add(new TableRow(tl2));

            // model status

            var l1 = new PixelLayout();

            var pb1 = new ImageView { Height = 40, Width = 40, Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-ok.png"), 40, 40, ImageInterpolation.Default) };
            var pb2 = new ImageView { Height = 40, Width = 40, Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-ok.png"), 40, 40, ImageInterpolation.Default) };
            var pb3 = new ImageView { Height = 40, Width = 40, Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-ok.png"), 40, 40, ImageInterpolation.Default) };

            var label1 = new Label { Height = 40, Text = "All Endpoint Material Streams are connected to Control Valves", Font = new Font(SystemFont.Default, UI.Shared.Common.GetEditorFontSize()), VerticalAlignment = VerticalAlignment.Center };
            var label2 = new Label { Height = 40, Text = "All Control Valves are configured with Kv Calculation Modes", Font = new Font(SystemFont.Default, UI.Shared.Common.GetEditorFontSize()), VerticalAlignment = VerticalAlignment.Center };
            var label3 = new Label { Height = 40, Text = "All Unit Operations in Flowsheet support Dynamic Mode", Font = new Font(SystemFont.Default, UI.Shared.Common.GetEditorFontSize()), VerticalAlignment = VerticalAlignment.Center };

            l1.Add(pb1, 20, 20);
            l1.Add(pb2, 20, 70);
            l1.Add(pb3, 20, 120);

            l1.Add(label1, 70, 20);
            l1.Add(label2, 70, 70);
            l1.Add(label3, 70, 120);

            DocumentContainer.Pages[0].Content = l1;

            // event sets

            var lce = new TableLayout();
            var rce = new TableLayout();
            var rce2 = new TableLayout();

            var btnAddEventSet = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Add New Set", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-plus_math.png")).WithSize(16, 16) };
            var btnRemoveEventSet = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Remove Selected Set", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-delete.png")).WithSize(16, 16) };

            btnAddEventSet.Click += (s, e) =>
            {
                try
                {
                    var es = new EventSet { ID = Guid.NewGuid().ToString() };
                    Dialog form = null;
                    var tb = new TextBox { Text = es.Description };
                    form = ext.CreateDialogWithButtons(tb, "Enter a Name", () => es.Description = tb.Text);
                    form.Location = btnAddEventSet.Location;
                    form.ShowModal(this);
                    lbEventSets.Items.Add(new ListItem { Key = es.ID, Text = es.Description });
                    Flowsheet.DynamicsManager.EventSetList.Add(es.ID, es);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                }
            };

            btnRemoveEventSet.Click += (s, e) =>
            {
                var result = MessageBox.Show("Confirm?", "Remove Event Set", MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Flowsheet.DynamicsManager.EventSetList.Remove(lbEventSets.SelectedKey);
                        lbEventSets.Items.RemoveAt(lbEventSets.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                    }
                }
            };

            lbEventSets = new ListBox();

            lce.Rows.Add(new Label { Text = "Event Sets", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var menu1 = new StackLayout
            {
                Items = { btnAddEventSet, btnRemoveEventSet },
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding = 5,
                Height = 34
            };
            lce.Rows.Add(new TableRow(menu1));

            lce.Rows.Add(new TableRow(lbEventSets));
            lce.Padding = new Padding(5, 5, 5, 5);
            lce.Spacing = new Size(0, 0);
            lce.Width = 250;

            rce.Rows.Add(new Label { Text = "Selected Event Set", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var btnAddEvent = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Add New Event", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-plus_math.png")).WithSize(16, 16) };
            var btnRemoveEvent = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Remove Selected Event", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-delete.png")).WithSize(16, 16) };

            btnAddEvent.Click += (s, e) =>
            {
                try
                {
                    var ev = new DynamicEvent { ID = Guid.NewGuid().ToString() };
                    Dialog form = null;
                    var tb = new TextBox { Text = ev.Description };
                    form = ext.CreateDialogWithButtons(tb, "Enter a Name", () => ev.Description = tb.Text);
                    form.Location = btnAddEvent.Location;
                    form.ShowModal(this);
                    lbEvents.Items.Add(new ListItem { Key = ev.ID, Text = ev.Description });
                    Flowsheet.DynamicsManager.EventSetList[lbEventSets.SelectedKey].Events.Add(ev.ID, ev);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                }
            };

            btnRemoveEvent.Click += (s, e) =>
            {
                var result = MessageBox.Show("Confirm?", "Remove Event", MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Flowsheet.DynamicsManager.EventSetList[lbEventSets.SelectedKey].Events.Remove(lbEvents.SelectedKey);
                        lbEvents.Items.RemoveAt(lbEvents.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                    }
                }
            };

            var menu2 = new StackLayout
            {
                Items = { btnAddEvent, btnRemoveEvent },
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding = 5,
                Height = 34
            };
            rce.Rows.Add(new TableRow(menu2));

            lbEvents = new ListBox();

            rce.Rows.Add(new TableRow(lbEvents));
            rce.Padding = new Padding(5, 5, 5, 5);

            rce2.Rows.Add(new Label { Text = "Selected Event", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            eventEditor = new DynamicLayout();

            rce2.Rows.Add(new TableRow(eventEditor));
            rce2.Padding = new Padding(5, 5, 5, 5);

            var splites2 = new Splitter() { };
            splites2.Panel1 = rce;
            splites2.Panel1.Width = 250;
            splites2.Panel2 = rce2;
            splites2.SplitterWidth = 2;

            var splites = new Splitter() { };
            splites.Panel1 = lce;
            splites.Panel1.Width = 250;
            splites.Panel2 = splites2;
            splites.SplitterWidth = 2;

            DocumentContainer.Pages[1].Content = splites;

            // cause and effect matrices

            var lcce = new TableLayout();
            var rcce = new TableLayout();
            var rcce2 = new TableLayout();

            var btnAddCEM = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Add New Set", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-plus_math.png")).WithSize(16, 16) };
            var btnRemoveCEM = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Remove Selected Set", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-delete.png")).WithSize(16, 16) };

            btnAddCEM.Click += (s, e) =>
            {
                try
                {
                    var es = new CauseAndEffectMatrix { ID = Guid.NewGuid().ToString() };
                    Dialog form = null;
                    var tb = new TextBox { Text = es.Description };
                    form = ext.CreateDialogWithButtons(tb, "Enter a Name", () => es.Description = tb.Text);
                    form.Location = btnAddCEM.Location;
                    form.ShowModal(this);
                    lbCEM.Items.Add(new ListItem { Key = es.ID, Text = es.Description });
                    Flowsheet.DynamicsManager.CauseAndEffectMatrixList.Add(es.ID, es);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                }
            };

            btnRemoveCEM.Click += (s, e) =>
            {
                var result = MessageBox.Show("Confirm?", "Remove Cause-and-Effect Matrix", MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Flowsheet.DynamicsManager.CauseAndEffectMatrixList.Remove(lbCEM.SelectedKey);
                        lbCEM.Items.RemoveAt(lbCEM.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                    }
                }
            };

            lbCEM = new ListBox();

            lcce.Rows.Add(new Label { Text = "Cause-and-Effect Matrices", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var menu3 = new StackLayout
            {
                Items = { btnAddCEM, btnRemoveCEM },
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding = 5,
                Height = 34
            };
            lcce.Rows.Add(new TableRow(menu3));

            lcce.Rows.Add(new TableRow(lbCEM));
            lcce.Padding = new Padding(5, 5, 5, 5);
            lcce.Spacing = new Size(0, 0);
            lcce.Width = 250;

            rcce.Rows.Add(new Label { Text = "Selected Cause-and-Effect Matrix", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var btnAddCEI = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Add New Event", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-plus_math.png")).WithSize(16, 16) };
            var btnRemoveCEI = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Remove Selected Event", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-delete.png")).WithSize(16, 16) };

            btnAddCEI.Click += (s, e) =>
            {
                try
                {
                    var ev = new CauseAndEffectItem { ID = Guid.NewGuid().ToString() };
                    Dialog form = null;
                    var tb = new TextBox { Text = ev.Description };
                    form = ext.CreateDialogWithButtons(tb, "Enter a Name", () => ev.Description = tb.Text);
                    form.Location = btnAddCEI.Location;
                    form.ShowModal(this);
                    lbCEI.Items.Add(new ListItem { Key = ev.ID, Text = ev.Description });
                    Flowsheet.DynamicsManager.CauseAndEffectMatrixList[lbCEM.SelectedKey].Items.Add(ev.ID, ev);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                }
            };

            btnRemoveCEI.Click += (s, e) =>
            {
                var result = MessageBox.Show("Confirm?", "Remove Cause-and-Effect Item", MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Flowsheet.DynamicsManager.CauseAndEffectMatrixList[lbCEM.SelectedKey].Items.Remove(lbCEI.SelectedKey);
                        lbCEI.Items.RemoveAt(lbCEI.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                    }
                }
            };

            var menu4 = new StackLayout
            {
                Items = { btnAddCEI, btnRemoveCEI },
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding = 5,
                Height = 34
            };
            rcce.Rows.Add(new TableRow(menu4));

            lbCEI = new ListBox();

            rcce.Rows.Add(new TableRow(lbCEI));
            rcce.Padding = new Padding(5, 5, 5, 5);

            rcce2.Rows.Add(new Label { Text = "Selected Item", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            ceiEditor = new DynamicLayout();

            rcce2.Rows.Add(new TableRow(ceiEditor));
            rcce2.Padding = new Padding(5, 5, 5, 5);

            var splites2a = new Splitter() { };
            splites2a.Panel1 = rcce;
            splites2a.Panel1.Width = 250;
            splites2a.Panel2 = rcce2;
            splites2a.SplitterWidth = 2;

            var splitesa = new Splitter() { };
            splitesa.Panel1 = lcce;
            splitesa.Panel1.Width = 250;
            splitesa.Panel2 = splites2a;
            splitesa.SplitterWidth = 2;

            DocumentContainer.Pages[2].Content = splitesa;

            // integrators

            var lcci = new TableLayout();
            var rcci = new TableLayout();

            var btnAddI = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Add New Set", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-plus_math.png")).WithSize(16, 16) };
            var btnRemoveI = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Remove Selected Set", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-delete.png")).WithSize(16, 16) };

            btnAddI.Click += (s, e) =>
            {
                try
                {
                    var es = new Integrator { ID = Guid.NewGuid().ToString() };
                    Dialog form = null;
                    var tb = new TextBox { Text = es.Description };
                    form = ext.CreateDialogWithButtons(tb, "Enter a Name", () => es.Description = tb.Text);
                    form.Location = btnAddI.Location;
                    form.ShowModal(this);
                    lbIntegrators.Items.Add(new ListItem { Key = es.ID, Text = es.Description });
                    Flowsheet.DynamicsManager.IntegratorList.Add(es.ID, es);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                }
            };

            btnRemoveI.Click += (s, e) =>
            {
                var result = MessageBox.Show("Confirm?", "Remove Integrator", MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Flowsheet.DynamicsManager.IntegratorList.Remove(lbIntegrators.SelectedKey);
                        lbIntegrators.Items.RemoveAt(lbIntegrators.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                    }
                }
            };

            lbIntegrators = new ListBox();

            lcci.Rows.Add(new Label { Text = "Integrators", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var menu5 = new StackLayout
            {
                Items = { btnAddI, btnRemoveI },
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding = 5,
                Height = 34
            };
            lcci.Rows.Add(new TableRow(menu5));

            lcci.Rows.Add(new TableRow(lbIntegrators));
            lcci.Padding = new Padding(5, 5, 5, 5);
            lcci.Spacing = new Size(0, 0);
            lcci.Width = 250;

            rcci.Rows.Add(new Label { Text = "Selected Integrator", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var docc1 = new DocumentControl();
            var docp1 = new DocumentPage { Text = "Parameters", Closable = false };
            var docp2 = new DocumentPage { Text = "Monitored Variables", Closable = false };

            rcci.Rows.Add(docc1);

            // monitored variables

            var lcv = new TableLayout();
            var rcv = new TableLayout();

            var btnAddVar = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Add New Variable", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-plus_math.png")).WithSize(16, 16) };
            var btnRemoveVar = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Remove Selected Variable", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-delete.png")).WithSize(16, 16) };

            btnAddVar.Click += (s, e) =>
            {
                try
                {
                    var es = new MonitoredVariable { ID = Guid.NewGuid().ToString() };
                    Dialog form = null;
                    var tb = new TextBox { Text = es.Description };
                    form = ext.CreateDialogWithButtons(tb, "Enter a Name", () => es.Description = tb.Text);
                    form.Location = btnAddVar.Location;
                    form.ShowModal(this);
                    lbVariables.Items.Add(new ListItem { Key = es.ID, Text = es.Description });
                    Flowsheet.DynamicsManager.IntegratorList[lbIntegrators.SelectedKey].MonitoredVariables.Add(es);               
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                }
            };

            btnRemoveVar.Click += (s, e) =>
            {
                var result = MessageBox.Show("Confirm?", "Remove Variable", MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Flowsheet.DynamicsManager.IntegratorList[lbIntegrators.SelectedKey].MonitoredVariables.RemoveAt(lbIntegrators.SelectedIndex);
                        lbVariables.Items.RemoveAt(lbVariables.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                    }
                }
            };

            lbVariables = new ListBox();

            lcv.Rows.Add(new Label { Text = "Monitored Variables", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var menu6 = new StackLayout
            {
                Items = { btnAddVar, btnRemoveVar },
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding = 5,
                Height = 34
            };
            lcv.Rows.Add(new TableRow(menu6));

            lcv.Rows.Add(new TableRow(lbVariables));
            lcv.Padding = new Padding(5, 5, 5, 5);
            lcv.Spacing = new Size(0, 0);
            lcv.Width = 250;

            rcv.Rows.Add(new Label { Text = "Selected Variable", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            mvEditor = new DynamicLayout();

            rcv.Rows.Add(new TableRow(mvEditor));
            rcv.Padding = new Padding(5, 5, 5, 5);

            var splites2b1 = new Splitter() { };
            splites2b1.Panel1 = lcv;
            splites2b1.Panel1.Width = 250;
            splites2b1.Panel2 = rcv;
            splites2b1.SplitterWidth = 2;

            docp2.Content = splites2b1;

            /////

            docc1.Pages.Add(docp1);
            docc1.Pages.Add(docp2);

            var splitesb = new Splitter() { };
            splitesb.Panel1 = lcci;
            splitesb.Panel1.Width = 250;
            splitesb.Panel2 = rcci;
            splitesb.SplitterWidth = 2;

            DocumentContainer.Pages[3].Content = splitesb;

            // schedules

            var lcs = new TableLayout();
            var rcs = new TableLayout();

            var btnAddS = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Add New Schedule", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-plus_math.png")).WithSize(16, 16) };
            var btnRemoveS = new Button() { ImagePosition = ButtonImagePosition.Overlay, Height = 24, Width = 24, ToolTip = "Remove Selected Schedule", Image = new Bitmap(Eto.Drawing.Bitmap.FromResource(imgprefix + "icons8-delete.png")).WithSize(16, 16) };

            btnAddS.Click += (s, e) =>
            {
                try
                {
                    var es = new Schedule { ID = Guid.NewGuid().ToString() };
                    Dialog form = null;
                    var tb = new TextBox { Text = es.Description };
                    form = ext.CreateDialogWithButtons(tb, "Enter a Name", () => es.Description = tb.Text);
                    form.Location = btnAddS.Location;
                    form.ShowModal(this);
                    lbSchedules.Items.Add(new ListItem { Key = es.ID, Text = es.Description });
                    Flowsheet.DynamicsManager.ScheduleList.Add(es.ID, es);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                }
            };

            btnRemoveS.Click += (s, e) =>
            {
                var result = MessageBox.Show("Confirm?", "Remove Schedule", MessageBoxType.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Flowsheet.DynamicsManager.ScheduleList.Remove(lbSchedules.SelectedKey);
                        lbSchedules.Items.RemoveAt(lbSchedules.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, MessageBoxType.Error);
                    }
                }
            };

            lbSchedules = new ListBox();

            lcs.Rows.Add(new Label { Text = "Schedules", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            var menu7 = new StackLayout
            {
                Items = { btnAddS, btnRemoveS },
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding = 5,
                Height = 34
            };
            lcs.Rows.Add(new TableRow(menu7));

            lcs.Rows.Add(new TableRow(lbSchedules));
            lcs.Padding = new Padding(5, 5, 5, 5);
            lcs.Spacing = new Size(0, 0);
            lcs.Width = 250;

            rcs.Rows.Add(new Label { Text = "Selected Schedule", Font = new Font(SystemFont.Bold, UI.Shared.Common.GetEditorFontSize()), Height = 30, VerticalAlignment = VerticalAlignment.Center });

            schEditor = new DynamicLayout();

            rcs.Rows.Add(new TableRow(schEditor));
            rcs.Padding = new Padding(5, 5, 5, 5);

            var splites2s = new Splitter() { };
            splites2s.Panel1 = lcs;
            splites2s.Panel1.Width = 250;
            splites2s.Panel2 = rcs;
            splites2s.SplitterWidth = 2;

            DocumentContainer.Pages[4].Content = splites2s;

            // populate lists

            foreach (var es in Flowsheet.DynamicsManager.EventSetList)
            {
                lbEventSets.Items.Add(new ListItem { Key = es.Key, Text = es.Value.Description });
            }

            lbEventSets.SelectedIndexChanged += (s, e) =>
            {
                if (lbEventSets.SelectedIndex < 0) return;
                lbEvents.Items.Clear();
                var es = Flowsheet.DynamicsManager.EventSetList[lbEventSets.SelectedKey];
                foreach (var ev in es.Events)
                {
                    lbEvents.Items.Add(new ListItem { Key = ev.Key, Text = ev.Value.Description });
                }
            };

            lbEvents.SelectedIndexChanged += (s, e) =>
            {
                if (lbEvents.SelectedIndex < 0) return;
                var ev = Flowsheet.DynamicsManager.EventSetList[lbEventSets.SelectedKey].Events[lbEvents.SelectedKey];
                Flowsheet.RunCodeOnUIThread(() =>
                {
                    PopulateEventContainer(ev);
                });
            };

            foreach (var es in Flowsheet.DynamicsManager.CauseAndEffectMatrixList)
            {
                lbCEM.Items.Add(new ListItem { Key = es.Key, Text = es.Value.Description });
            }

            lbCEM.SelectedIndexChanged += (s, e) =>
            {
                if (lbCEM.SelectedIndex < 0) return;
                lbCEI.Items.Clear();
                var es = Flowsheet.DynamicsManager.CauseAndEffectMatrixList[lbCEM.SelectedKey];
                foreach (var ev in es.Items)
                {
                    lbCEI.Items.Add(new ListItem { Key = ev.Key, Text = ev.Value.Description });
                }
            };

            lbCEI.SelectedIndexChanged += (s, e) =>
            {
                if (lbCEI.SelectedIndex < 0) return;
                var ev = Flowsheet.DynamicsManager.CauseAndEffectMatrixList[lbCEM.SelectedKey].Items[lbCEI.SelectedKey];
                Flowsheet.RunCodeOnUIThread(() =>
                {
                    PopulateCEIContainer(ev);
                });
            };

            foreach (var es in Flowsheet.DynamicsManager.IntegratorList)
            {
                lbIntegrators.Items.Add(new ListItem { Key = es.Key, Text = es.Value.Description });
            }

            lbIntegrators.SelectedIndexChanged += (s, e) =>
            {
                if (lbIntegrators.SelectedIndex < 0) return;
                var integ = Flowsheet.DynamicsManager.IntegratorList[lbIntegrators.SelectedKey];
                Flowsheet.RunCodeOnUIThread(() =>
                {
                    PopulateIntegratorProperties(integ, docp1);
                });
                lbVariables.Items.Clear();
                foreach (var mv in integ.MonitoredVariables)
                {
                    lbVariables.Items.Add(new ListItem { Key = mv.ID, Text = mv.Description });
                }
            };

            lbVariables.SelectedIndexChanged += (s, e) =>
            {
                if (lbVariables.SelectedIndex < 0) return;
                var integ = Flowsheet.DynamicsManager.IntegratorList[lbIntegrators.SelectedKey];
                var mv = integ.MonitoredVariables[lbVariables.SelectedIndex];
                Flowsheet.RunCodeOnUIThread(() =>
                {
                    PopulateMonitoredVariable(mv);
                });
            };

            foreach (var es in Flowsheet.DynamicsManager.ScheduleList)
            {
                lbSchedules.Items.Add(new ListItem { Key = es.Key, Text = es.Value.Description });
            }

            lbSchedules.SelectedIndexChanged += (s, e) =>
            {
                if (lbSchedules.SelectedIndex < 0) return;
                var sch = Flowsheet.DynamicsManager.ScheduleList[lbSchedules.SelectedKey];
                Flowsheet.RunCodeOnUIThread(() =>
                {
                    PopulateScheduleProperties(sch);
                });
            };

            //if (lbEventSets.Items.Count > 0) lbEventSets.SelectedIndex = 0;
            //if (lbCEM.Items.Count > 0) lbCEM.SelectedIndex = 0;
            //if (lbIntegrators.Items.Count > 0) lbIntegrators.SelectedIndex = 0;
            //if (lbSchedules.Items.Count > 0) lbSchedules.SelectedIndex = 0;

        }

        private void PopulateScheduleProperties(IDynamicsSchedule sch)
        {

            var layout = ext.GetDefaultContainer();

            var integrators = Flowsheet.DynamicsManager.IntegratorList.Values.ToList();
            var integratorIDs = integrators.Select((x) => x.ID).ToList();
            var integratorNames = integrators.Select((x) => x.Description).ToList();
            integratorIDs.Insert(0, "");
            integratorNames.Insert(0, "");

            layout.CreateAndAddDropDownRow("Selected Integrator", integratorNames, integratorIDs.IndexOf(sch.CurrentIntegrator), (dd, e) => {
                sch.CurrentIntegrator = integratorIDs[dd.SelectedIndex];
            });

            layout.CreateAndAddEmptySpace();
            layout.CreateAndAddEmptySpace();

            layout.CreateAndAddCheckBoxRow("Uses Event Set", sch.UsesEventList, (chk, e) => {
                sch.UsesEventList = chk.Checked.GetValueOrDefault();
            });

            var events = Flowsheet.DynamicsManager.EventSetList.Values.ToList();
            var eventIDs = events.Select((x) => x.ID).ToList();
            var eventNames = events.Select((x) => x.Description).ToList();
            eventIDs.Insert(0, "");
            eventNames.Insert(0, "");

            layout.CreateAndAddDropDownRow("Selected Event Set", eventNames, eventIDs.IndexOf(sch.CurrentEventList), (dd, e) => {
                sch.CurrentEventList = eventIDs[dd.SelectedIndex];
            });

            layout.CreateAndAddEmptySpace();
            layout.CreateAndAddEmptySpace();

            layout.CreateAndAddCheckBoxRow("Uses Cause-and-Effect Matrix", sch.UsesCauseAndEffectMatrix, (chk, e) => {
                sch.UsesCauseAndEffectMatrix = chk.Checked.GetValueOrDefault();
            });

            var cem = Flowsheet.DynamicsManager.CauseAndEffectMatrixList.Values.ToList();
            var cemIDs = cem.Select((x) => x.ID).ToList();
            var cemNames = cem.Select((x) => x.Description).ToList();
            cemIDs.Insert(0, "");
            cemNames.Insert(0, "");

            layout.CreateAndAddDropDownRow("Selected Cause-and-Effect Matrix", cemNames, cemIDs.IndexOf(sch.CurrentCauseAndEffectMatrix), (dd, e) => {
                sch.CurrentCauseAndEffectMatrix = cemIDs[dd.SelectedIndex];
            });

            var fstates = Flowsheet.StoredSolutions.Keys.ToList();
            var fstateIDs = fstates.Select((x) => x).ToList();
            fstateIDs.Insert(0, "");

            layout.CreateAndAddEmptySpace();
            layout.CreateAndAddEmptySpace();

            layout.CreateAndAddDropDownRow("Initial Flowsheet State", fstateIDs, fstateIDs.IndexOf(sch.InitialFlowsheetStateID), (dd, e) => {
                sch.InitialFlowsheetStateID = fstateIDs[dd.SelectedIndex];
            });

            layout.CreateAndAddCheckBoxRow("Use Current State as Initial", sch.UseCurrentStateAsInitial, (chk, e) => {
                sch.UseCurrentStateAsInitial = chk.Checked.GetValueOrDefault();
            });

            layout.Padding = new Padding(10, 10, schEditor.Width / 2, 10);

            layout.Invalidate();

            schEditor.Content = layout;

        }

        private void PopulateMonitoredVariable(IDynamicsMonitoredVariable mv)
        {

            var layout = new DynamicLayout();

            layout.CreateAndAddStringEditorRow("Name", mv.Description, (s, e) =>
            {
                mv.Description = s.Text;
                lbVariables.Items[lbVariables.SelectedIndex].Text = s.Text;
            });

            var objects = Flowsheet.SimulationObjects.Values.Select((x) => x.GraphicObject.Tag).ToList();
            objects.Insert(0, "");

            DropDown propselector = null;
            List<string> props = new List<string>();
            List<string> propids = new List<string>();
            propids.Add("");

            int idx = 0;

            if (mv.ObjectID != "")
            {
                if (Flowsheet.SimulationObjects.ContainsKey(mv.ObjectID))
                {
                    idx = objects.IndexOf(Flowsheet.SimulationObjects[mv.ObjectID].GraphicObject.Tag);
                    propids.AddRange(Flowsheet.SimulationObjects[mv.ObjectID].GetProperties(Interfaces.Enums.PropertyType.WR));
                    props.AddRange(propids.Select((x) => Flowsheet.GetTranslatedString(x)).ToArray());
                }
            }

            layout.CreateAndAddDropDownRow("Object", objects, idx, (s, e) =>
            {
                if (s.SelectedIndex != 0)
                {
                    mv.ObjectID = Flowsheet.GetFlowsheetSimulationObject(s.SelectedValue.ToString()).Name;
                    propids.Clear();
                    propids.Add("");
                    propids.AddRange(Flowsheet.SimulationObjects[mv.ObjectID].GetProperties(Interfaces.Enums.PropertyType.WR));
                    props.Clear();
                    props.AddRange(propids.Select((x) => Flowsheet.GetTranslatedString(x)).ToArray());
                    propselector.Items.Clear();
                    propselector.Items.AddRange(props.Select((x) => new ListItem { Text = x }));
                }
                else
                {
                    mv.ObjectID = "";
                }
            });

            propselector = layout.CreateAndAddDropDownRow("Property", props,
                props.IndexOf(Flowsheet.GetTranslatedString(mv.PropertyID)),
                (s, e) =>
                {
                    if (s.SelectedIndex >= 0) mv.PropertyID = propids[s.SelectedIndex];
                });

            layout.CreateAndAddStringEditorRow("Units",
                mv.PropertyUnits, (s, e) =>
                {
                    mv.PropertyUnits = s.Text;
                });

            layout.Padding = new Padding(10, 10, mvEditor.Width / 3, 10);

            layout.Invalidate();

            mvEditor.Content = layout;

        }

        private void PopulateIntegratorProperties(IDynamicsIntegrator integ, DocumentPage page)
        {

            var layout = ext.GetDefaultContainer();

            layout.CreateAndAddLabelRow("Properties");

            var dtp = new DateTimePicker { Mode = DateTimePickerMode.Time, Value = new DateTime().Add(integ.Duration) };
            dtp.Font = new Font(SystemFont.Default, UI.Shared.Common.GetEditorFontSize());
            dtp.ValueChanged += (s, e) =>
            {
                integ.Duration = dtp.Value.GetValueOrDefault().Subtract(new DateTime());
            };

            layout.CreateAndAddLabelAndControlRow("Duration", dtp);

            var dtp2 = new DateTimePicker { Mode = DateTimePickerMode.Time, Value = new DateTime().Add(integ.IntegrationStep) };
            dtp2.Font = new Font(SystemFont.Default, UI.Shared.Common.GetEditorFontSize());
            dtp2.ValueChanged += (s, e) =>
            {
                integ.IntegrationStep = dtp2.Value.GetValueOrDefault().Subtract(new DateTime());
            };

            layout.CreateAndAddLabelAndControlRow("Integration Step", dtp2);

            layout.CreateAndAddLabelRow("Calculation Rates");

            layout.CreateAndAddNumericEditorRow("Equilibrium Flash", integ.CalculationRateEquilibrium, 1, 100, 0, (s, e) => {
                integ.CalculationRateEquilibrium = (int)s.Value;
            });

            layout.CreateAndAddNumericEditorRow("Pressure-Flow Relations", integ.CalculationRatePressureFlow, 1, 100, 0, (s, e) => {
                integ.CalculationRatePressureFlow = (int)s.Value;
            });

            layout.CreateAndAddNumericEditorRow("Controller Updates", integ.CalculationRateControl, 1, 100, 0, (s, e) => {
                integ.CalculationRateControl = (int)s.Value;
            });

            layout.Padding = new Padding(10, 10, page.Width/2, 10);

            layout.Invalidate();

            page.Content = layout;

        }

        public void PopulateEventContainer(Interfaces.IDynamicsEvent ev)
        {

            var layout = new DynamicLayout();

            layout.CreateAndAddCheckBoxRow("Active", ev.Enabled, (s, e) =>
            {
                ev.Enabled = s.Checked.GetValueOrDefault();
            });

            layout.CreateAndAddStringEditorRow("Name", ev.Description, (s, e) =>
            {
                ev.Description = s.Text;
                lbEvents.Items[lbEvents.SelectedIndex].Text = s.Text;
            });

            var dtp = new DateTimePicker { Mode = DateTimePickerMode.Time, Value = ev.TimeStamp };
            dtp.Font = new Font(SystemFont.Default, UI.Shared.Common.GetEditorFontSize());
            dtp.ValueChanged += (s, e) =>
            {
                ev.TimeStamp = dtp.Value.GetValueOrDefault();
            };

            layout.CreateAndAddLabelAndControlRow("Timestamp", dtp);

            layout.CreateAndAddDropDownRow("Type", ev.EventType.GetEnumNames(), (int)ev.EventType, (s, e) =>
            {
                ev.EventType = s.SelectedIndex.ToEnum<Interfaces.Enums.Dynamics.DynamicsEventType>();
            });

            var objects = Flowsheet.SimulationObjects.Values.Select((x) => x.GraphicObject.Tag).ToList();
            objects.Insert(0, "");

            DropDown propselector = null;
            List<string> props = new List<string>();
            List<string> propids = new List<string>();
            propids.Add("");

            int idx = 0;

            if (ev.SimulationObjectID != "")
            {
                if (Flowsheet.SimulationObjects.ContainsKey(ev.SimulationObjectID))
                {
                    idx = objects.IndexOf(Flowsheet.SimulationObjects[ev.SimulationObjectID].GraphicObject.Tag);
                    propids.AddRange(Flowsheet.SimulationObjects[ev.SimulationObjectID].GetProperties(Interfaces.Enums.PropertyType.WR));
                    props.AddRange(propids.Select((x) => Flowsheet.GetTranslatedString(x)).ToArray());
                }
            }

            layout.CreateAndAddDropDownRow("Object", objects, idx, (s, e) =>
            {
                if (s.SelectedIndex != 0)
                {
                    ev.SimulationObjectID = Flowsheet.GetFlowsheetSimulationObject(s.SelectedValue.ToString()).Name;
                    propids.Clear();
                    propids.Add("");
                    propids.AddRange(Flowsheet.SimulationObjects[ev.SimulationObjectID].GetProperties(Interfaces.Enums.PropertyType.WR));
                    props.Clear();
                    props.AddRange(propids.Select((x) => Flowsheet.GetTranslatedString(x)).ToArray());
                    propselector.Items.Clear();
                    propselector.Items.AddRange(props.Select((x) => new ListItem { Text = x }));
                }
                else
                {
                    ev.SimulationObjectID = "";
                }
            });

            propselector = layout.CreateAndAddDropDownRow("Property", props,
                props.IndexOf(Flowsheet.GetTranslatedString(ev.SimulationObjectProperty)),
                (s, e) =>
                {
                    if (s.SelectedIndex >= 0) ev.SimulationObjectProperty = propids[s.SelectedIndex];
                });

            layout.CreateAndAddStringEditorRow("Value",
                ev.SimulationObjectPropertyValue, (s, e) =>
                {
                    ev.SimulationObjectPropertyValue = s.Text;
                });

            layout.CreateAndAddStringEditorRow("Units",
                ev.SimulationObjectPropertyUnits, (s, e) =>
                {
                    ev.SimulationObjectPropertyUnits = s.Text;
                });

            layout.Padding = new Padding(10, 10, eventEditor.Width / 3, 10);

            layout.Invalidate();

            eventEditor.Content = layout;

        }

        public void PopulateCEIContainer(Interfaces.IDynamicsCauseAndEffectItem ev)
        {

            var layout = new DynamicLayout();

            layout.CreateAndAddCheckBoxRow("Active", ev.Enabled, (s, e) =>
            {
                ev.Enabled = s.Checked.GetValueOrDefault();
            });

            layout.CreateAndAddStringEditorRow("Name", ev.Description, (s, e) =>
            {
                ev.Description = s.Text;
                lbEvents.Items[lbEvents.SelectedIndex].Text = s.Text;
            });

            var indicators = Flowsheet.SimulationObjects.Values.Where((x0) => x0.ObjectClass == Interfaces.Enums.SimulationObjectClass.Indicators).Select((x) => x.GraphicObject.Tag).ToList();
            indicators.Insert(0, "");

            int idxi = 0;

            if (ev.AssociatedIndicator != "")
            {
                if (Flowsheet.SimulationObjects.ContainsKey(ev.AssociatedIndicator))
                {
                    idxi = indicators.IndexOf(Flowsheet.SimulationObjects[ev.AssociatedIndicator].GraphicObject.Tag);                    
                }
            }

            layout.CreateAndAddDropDownRow("Indicator", indicators, idxi, (s, e) =>
            {
                if (s.SelectedIndex != 0)
                {
                    ev.AssociatedIndicator = Flowsheet.GetFlowsheetSimulationObject(s.SelectedValue.ToString()).Name;
                }
                else
                {
                    ev.AssociatedIndicator = "";
                }
            });

            layout.CreateAndAddDropDownRow("Type", ev.AssociatedIndicatorAlarm.GetEnumNames(), (int)ev.AssociatedIndicatorAlarm, (s, e) =>
            {
                ev.AssociatedIndicatorAlarm = s.SelectedIndex.ToEnum<Interfaces.Enums.Dynamics.DynamicsAlarmType>();
            });

            var objects = Flowsheet.SimulationObjects.Values.Select((x) => x.GraphicObject.Tag).ToList();
            objects.Insert(0, "");

            DropDown propselector = null;
            List<string> props = new List<string>();
            List<string> propids = new List<string>();
            propids.Add("");

            int idx = 0;

            if (ev.SimulationObjectID != "")
            {
                if (Flowsheet.SimulationObjects.ContainsKey(ev.SimulationObjectID))
                {
                    idx = objects.IndexOf(Flowsheet.SimulationObjects[ev.SimulationObjectID].GraphicObject.Tag);
                    propids.AddRange(Flowsheet.SimulationObjects[ev.SimulationObjectID].GetProperties(Interfaces.Enums.PropertyType.WR));
                    props.AddRange(propids.Select((x) => Flowsheet.GetTranslatedString(x)).ToArray());
                }
            }

            layout.CreateAndAddDropDownRow("Object", objects, idx, (s, e) =>
            {
                if (s.SelectedIndex != 0)
                {
                    ev.SimulationObjectID = Flowsheet.GetFlowsheetSimulationObject(s.SelectedValue.ToString()).Name;
                    propids.Clear();
                    propids.Add("");
                    propids.AddRange(Flowsheet.SimulationObjects[ev.SimulationObjectID].GetProperties(Interfaces.Enums.PropertyType.WR));
                    props.Clear();
                    props.AddRange(propids.Select((x) => Flowsheet.GetTranslatedString(x)).ToArray());
                    propselector.Items.Clear();
                    propselector.Items.AddRange(props.Select((x) => new ListItem { Text = x }));
                }
                else
                {
                    ev.SimulationObjectID = "";
                }
            });

            propselector = layout.CreateAndAddDropDownRow("Property", props,
                props.IndexOf(Flowsheet.GetTranslatedString(ev.SimulationObjectProperty)),
                (s, e) =>
                {
                    if (s.SelectedIndex >= 0) ev.SimulationObjectProperty = propids[s.SelectedIndex];
                });

            layout.CreateAndAddStringEditorRow("Value",
                ev.SimulationObjectPropertyValue, (s, e) =>
                {
                    ev.SimulationObjectPropertyValue = s.Text;
                });

            layout.CreateAndAddStringEditorRow("Units",
                ev.SimulationObjectPropertyUnits, (s, e) =>
                {
                    ev.SimulationObjectPropertyUnits = s.Text;
                });

            layout.Padding = new Padding(10, 10, ceiEditor.Width / 3, 10);

            layout.Invalidate();

            ceiEditor.Content = layout;

        }


    }
}
