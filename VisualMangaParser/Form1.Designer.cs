namespace VisualMangaParser
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.radStripView = new System.Windows.Forms.RadioButton();
            this.cboReadingOrder = new System.Windows.Forms.ComboBox();
            this.radCellViewOrder = new System.Windows.Forms.RadioButton();
            this.radReadingOrder = new System.Windows.Forms.RadioButton();
            this.radDependencyGraph = new System.Windows.Forms.RadioButton();
            this.radFilteredBoundingBoxes = new System.Windows.Forms.RadioButton();
            this.radMergedConvexHull = new System.Windows.Forms.RadioButton();
            this.radConvexHull = new System.Windows.Forms.RadioButton();
            this.radUnfilteredBoundingBoxes = new System.Windows.Forms.RadioButton();
            this.radThreshold = new System.Windows.Forms.RadioButton();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.AllowDrop = true;
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox2);
            this.splitContainer1.Size = new System.Drawing.Size(844, 390);
            this.splitContainer1.SplitterDistance = 434;
            this.splitContainer1.TabIndex = 2;
            this.splitContainer1.DragDrop += new System.Windows.Forms.DragEventHandler(this.splitContainer1_DragDrop);
            this.splitContainer1.DragEnter += new System.Windows.Forms.DragEventHandler(this.splitContainer1_DragEnter);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::VisualMangaParser.Properties.Resources.testcase;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(434, 390);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(406, 390);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.radStripView);
            this.panel1.Controls.Add(this.cboReadingOrder);
            this.panel1.Controls.Add(this.radCellViewOrder);
            this.panel1.Controls.Add(this.radReadingOrder);
            this.panel1.Controls.Add(this.radDependencyGraph);
            this.panel1.Controls.Add(this.radFilteredBoundingBoxes);
            this.panel1.Controls.Add(this.radMergedConvexHull);
            this.panel1.Controls.Add(this.radConvexHull);
            this.panel1.Controls.Add(this.radUnfilteredBoundingBoxes);
            this.panel1.Controls.Add(this.radThreshold);
            this.panel1.Controls.Add(this.trackBar1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 408);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(868, 100);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(698, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Normal";
            // 
            // radStripView
            // 
            this.radStripView.AutoSize = true;
            this.radStripView.Location = new System.Drawing.Point(698, 26);
            this.radStripView.Name = "radStripView";
            this.radStripView.Size = new System.Drawing.Size(101, 17);
            this.radStripView.TabIndex = 10;
            this.radStripView.TabStop = true;
            this.radStripView.Text = "Strip View Order";
            this.radStripView.UseVisualStyleBackColor = true;
            this.radStripView.CheckedChanged += new System.EventHandler(this.stagesChanged);
            // 
            // cboReadingOrder
            // 
            this.cboReadingOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboReadingOrder.FormattingEnabled = true;
            this.cboReadingOrder.Location = new System.Drawing.Point(459, 70);
            this.cboReadingOrder.Name = "cboReadingOrder";
            this.cboReadingOrder.Size = new System.Drawing.Size(170, 21);
            this.cboReadingOrder.TabIndex = 9;
            this.cboReadingOrder.SelectedIndexChanged += new System.EventHandler(this.stagesChanged);
            // 
            // radCellViewOrder
            // 
            this.radCellViewOrder.AutoSize = true;
            this.radCellViewOrder.Location = new System.Drawing.Point(698, 3);
            this.radCellViewOrder.Name = "radCellViewOrder";
            this.radCellViewOrder.Size = new System.Drawing.Size(97, 17);
            this.radCellViewOrder.TabIndex = 8;
            this.radCellViewOrder.TabStop = true;
            this.radCellViewOrder.Text = "Cell View Order";
            this.radCellViewOrder.UseVisualStyleBackColor = true;
            this.radCellViewOrder.CheckedChanged += new System.EventHandler(this.stagesChanged);
            // 
            // radReadingOrder
            // 
            this.radReadingOrder.AutoSize = true;
            this.radReadingOrder.Location = new System.Drawing.Point(436, 49);
            this.radReadingOrder.Name = "radReadingOrder";
            this.radReadingOrder.Size = new System.Drawing.Size(112, 17);
            this.radReadingOrder.TabIndex = 7;
            this.radReadingOrder.TabStop = true;
            this.radReadingOrder.Text = "Cells reading order";
            this.radReadingOrder.UseVisualStyleBackColor = true;
            this.radReadingOrder.CheckedChanged += new System.EventHandler(this.stagesChanged);
            // 
            // radDependencyGraph
            // 
            this.radDependencyGraph.AutoSize = true;
            this.radDependencyGraph.Location = new System.Drawing.Point(436, 26);
            this.radDependencyGraph.Name = "radDependencyGraph";
            this.radDependencyGraph.Size = new System.Drawing.Size(139, 17);
            this.radDependencyGraph.TabIndex = 6;
            this.radDependencyGraph.TabStop = true;
            this.radDependencyGraph.Text = "Cells dependency graph";
            this.radDependencyGraph.UseVisualStyleBackColor = true;
            this.radDependencyGraph.CheckedChanged += new System.EventHandler(this.stagesChanged);
            // 
            // radFilteredBoundingBoxes
            // 
            this.radFilteredBoundingBoxes.AutoSize = true;
            this.radFilteredBoundingBoxes.Location = new System.Drawing.Point(12, 48);
            this.radFilteredBoundingBoxes.Name = "radFilteredBoundingBoxes";
            this.radFilteredBoundingBoxes.Size = new System.Drawing.Size(139, 17);
            this.radFilteredBoundingBoxes.TabIndex = 5;
            this.radFilteredBoundingBoxes.Tag = "1";
            this.radFilteredBoundingBoxes.Text = "Filtered Bounding Boxes";
            this.radFilteredBoundingBoxes.UseVisualStyleBackColor = true;
            this.radFilteredBoundingBoxes.CheckedChanged += new System.EventHandler(this.stagesChanged);
            this.radFilteredBoundingBoxes.Click += new System.EventHandler(this.stagesChanged);
            // 
            // radMergedConvexHull
            // 
            this.radMergedConvexHull.AutoSize = true;
            this.radMergedConvexHull.Location = new System.Drawing.Point(436, 3);
            this.radMergedConvexHull.Name = "radMergedConvexHull";
            this.radMergedConvexHull.Size = new System.Drawing.Size(124, 17);
            this.radMergedConvexHull.TabIndex = 4;
            this.radMergedConvexHull.Tag = "1";
            this.radMergedConvexHull.Text = "Merged Convex hulls";
            this.radMergedConvexHull.UseVisualStyleBackColor = true;
            this.radMergedConvexHull.CheckedChanged += new System.EventHandler(this.stagesChanged);
            // 
            // radConvexHull
            // 
            this.radConvexHull.AutoSize = true;
            this.radConvexHull.Location = new System.Drawing.Point(12, 71);
            this.radConvexHull.Name = "radConvexHull";
            this.radConvexHull.Size = new System.Drawing.Size(85, 17);
            this.radConvexHull.TabIndex = 4;
            this.radConvexHull.Tag = "1";
            this.radConvexHull.Text = "Convex hulls";
            this.radConvexHull.UseVisualStyleBackColor = true;
            this.radConvexHull.Click += new System.EventHandler(this.stagesChanged);
            // 
            // radUnfilteredBoundingBoxes
            // 
            this.radUnfilteredBoundingBoxes.AutoSize = true;
            this.radUnfilteredBoundingBoxes.Location = new System.Drawing.Point(12, 26);
            this.radUnfilteredBoundingBoxes.Name = "radUnfilteredBoundingBoxes";
            this.radUnfilteredBoundingBoxes.Size = new System.Drawing.Size(150, 17);
            this.radUnfilteredBoundingBoxes.TabIndex = 3;
            this.radUnfilteredBoundingBoxes.Tag = "1";
            this.radUnfilteredBoundingBoxes.Text = "Unfiltered Bounding Boxes";
            this.radUnfilteredBoundingBoxes.UseVisualStyleBackColor = true;
            this.radUnfilteredBoundingBoxes.CheckedChanged += new System.EventHandler(this.stagesChanged);
            // 
            // radThreshold
            // 
            this.radThreshold.AutoSize = true;
            this.radThreshold.Checked = true;
            this.radThreshold.Location = new System.Drawing.Point(12, 3);
            this.radThreshold.Name = "radThreshold";
            this.radThreshold.Size = new System.Drawing.Size(72, 17);
            this.radThreshold.TabIndex = 2;
            this.radThreshold.TabStop = true;
            this.radThreshold.Tag = "0";
            this.radThreshold.Text = "Threshold";
            this.radThreshold.UseVisualStyleBackColor = true;
            this.radThreshold.CheckedChanged += new System.EventHandler(this.stagesChanged);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(90, 3);
            this.trackBar1.Maximum = 255;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(240, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar1.Value = 4;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 508);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RadioButton radUnfilteredBoundingBoxes;
        private System.Windows.Forms.RadioButton radThreshold;
        private System.Windows.Forms.RadioButton radConvexHull;
        private System.Windows.Forms.RadioButton radFilteredBoundingBoxes;
        private System.Windows.Forms.RadioButton radMergedConvexHull;
        private System.Windows.Forms.RadioButton radDependencyGraph;
        private System.Windows.Forms.RadioButton radReadingOrder;
        private System.Windows.Forms.RadioButton radCellViewOrder;
        private System.Windows.Forms.ComboBox cboReadingOrder;
        private System.Windows.Forms.RadioButton radStripView;
        private System.Windows.Forms.Label label1;
    }
}

