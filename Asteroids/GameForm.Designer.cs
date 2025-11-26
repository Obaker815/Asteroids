namespace Asteroids
{
    partial class GameForm
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
            SuspendLayout();
            // 
            // GameForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            DoubleBuffered = true;
            Name = "GameForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Asteroids";
            FormClosing += GameForm_FormClosing;
            Load += GameForm_Load;
            Shown += GameForm_Shown;
            Paint += GameForm_Paint;
            GotFocus += GameForm_GotFocus;
            KeyDown += GameForm_KeyDown;
            KeyUp += GameForm_KeyUp;
            ResumeLayout(false);
        }

        #endregion
    }
}