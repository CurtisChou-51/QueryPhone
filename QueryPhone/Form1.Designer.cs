namespace QueryPhone
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			btnQueryPhone = new Button();
			txtPhone = new TextBox();
			txtResult = new TextBox();
			clientsCheckedListBox = new CheckedListBox();
			SuspendLayout();
			// 
			// btnQueryPhone
			// 
			btnQueryPhone.Location = new Point(664, 118);
			btnQueryPhone.Name = "btnQueryPhone";
			btnQueryPhone.Size = new Size(86, 28);
			btnQueryPhone.TabIndex = 0;
			btnQueryPhone.Text = "button1";
			btnQueryPhone.UseVisualStyleBackColor = true;
			btnQueryPhone.Click += btnQueryPhone_Click;
			// 
			// txtPhone
			// 
			txtPhone.Location = new Point(36, 118);
			txtPhone.Name = "txtPhone";
			txtPhone.Size = new Size(610, 23);
			txtPhone.TabIndex = 1;
			// 
			// txtResult
			// 
			txtResult.Location = new Point(36, 162);
			txtResult.Multiline = true;
			txtResult.Name = "txtResult";
			txtResult.ScrollBars = ScrollBars.Vertical;
			txtResult.Size = new Size(718, 360);
			txtResult.TabIndex = 2;
			// 
			// clientsCheckedListBox
			// 
			clientsCheckedListBox.CheckOnClick = true;
			clientsCheckedListBox.FormattingEnabled = true;
			clientsCheckedListBox.Location = new Point(36, 24);
			clientsCheckedListBox.Name = "clientsCheckedListBox";
			clientsCheckedListBox.Size = new Size(716, 76);
			clientsCheckedListBox.TabIndex = 3;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(796, 552);
			Controls.Add(clientsCheckedListBox);
			Controls.Add(txtResult);
			Controls.Add(txtPhone);
			Controls.Add(btnQueryPhone);
			Name = "Form1";
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button btnQueryPhone;
		private TextBox txtPhone;
		private TextBox txtResult;
		private CheckedListBox clientsCheckedListBox;
	}
}
