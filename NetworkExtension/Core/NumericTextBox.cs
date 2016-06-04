using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Futura.ArcGIS.NetworkExtension
{

    public class NumericTextBox : TextBox
    {
        #region Variables
        bool allowSpace = false;
        #endregion

        #region Overridden Methods
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            NumberFormatInfo numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            string groupSeparator = numberFormatInfo.NumberGroupSeparator;
            string negativeSign = numberFormatInfo.NegativeSign;
            string keyInput = e.KeyChar.ToString();

            if (Char.IsDigit(e.KeyChar))
            {
            }
            else if (keyInput.Equals(decimalSeparator) || keyInput.Equals(groupSeparator) || keyInput.Equals(negativeSign))
            {
                // Decimal separator is OK
            }
            else if (e.KeyChar == '\b')
            {
                // Backspace key is OK
            }
            else if (this.allowSpace && e.KeyChar == ' ')
            {

            }
            else
                e.Handled = true;
        }

        #endregion

        #region Public Properties
        public int IntValue
        {
            get { return Int32.Parse(this.Text); }
        }

        public decimal DecimalValue
        {
            get { return Decimal.Parse(this.Text); }
        }

        public double DoubleValue
        {
            get { return DatabaseUtil.ToDouble(this.Text); }
        }

        public bool AllowSpace
        {
            set { this.allowSpace = value; }
            get { return this.allowSpace; }
        }
        #endregion
    }
}
