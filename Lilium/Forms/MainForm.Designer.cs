namespace Lilium
{
	partial class MainForm
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
			this.InfoContainer = new System.Windows.Forms.FlowLayoutPanel();
			this.lbObjects = new System.Windows.Forms.ListBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.InfoContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.InfoContainer);
			this.splitContainer1.Size = new System.Drawing.Size(1264, 761);
			this.splitContainer1.SplitterDistance = 876;
			this.splitContainer1.SplitterWidth = 1;
			this.splitContainer1.TabIndex = 1;
			// 
			// InfoContainer
			// 
			this.InfoContainer.AutoScroll = true;
			this.InfoContainer.AutoSize = true;
			this.InfoContainer.Controls.Add(this.lbObjects);
			this.InfoContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InfoContainer.Location = new System.Drawing.Point(0, 0);
			this.InfoContainer.Margin = new System.Windows.Forms.Padding(0);
			this.InfoContainer.MaximumSize = new System.Drawing.Size(9999, 9999);
			this.InfoContainer.Name = "InfoContainer";
			this.InfoContainer.Size = new System.Drawing.Size(385, 759);
			this.InfoContainer.TabIndex = 2;
			// 
			// lbObjects
			// 
			this.lbObjects.FormattingEnabled = true;
			this.lbObjects.ItemHeight = 12;
			this.lbObjects.Location = new System.Drawing.Point(3, 3);
			this.lbObjects.Name = "lbObjects";
			this.lbObjects.Size = new System.Drawing.Size(360, 148);
			this.lbObjects.TabIndex = 0;
			this.lbObjects.SelectedIndexChanged += new System.EventHandler(this.lbObjects_SelectedIndexChanged);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1264, 761);
			this.Controls.Add(this.splitContainer1);
			this.Name = "MainForm";
			this.Text = "MainForm";
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.InfoContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		public System.Windows.Forms.FlowLayoutPanel InfoContainer;
		private System.Windows.Forms.ListBox lbObjects;
	}
}