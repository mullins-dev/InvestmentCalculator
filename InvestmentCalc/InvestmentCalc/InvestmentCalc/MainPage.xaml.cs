using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace InvestmentCalc
{
	public partial class MainPage : ContentPage
	{
		// Various stacks to hold operations in memory
		private Stack<int> additionStack = new Stack<int>();
		private Stack<int> subtractionStack = new Stack<int>();
		private Stack<int> divideStack = new Stack<int>();
		private Stack<int> multiplyStack = new Stack<int>();

		// Integer which holds the value of the memory
		private int memory = 0;

		public MainPage()
		{
			InitializeComponent();
		}

		//=============================================================================================== Memory functions

		/*
		 * Handles functions which deal with the memory
		 */
		private void Memory_Clicked(object sender, EventArgs e)
		{
			Button b = (Button)sender;

			if (b.Text.Equals("MC"))
			{
				this.memory = 0;
			}
			else if (b.Text.Equals("MR"))
			{
				MainScreen.Text = memory.ToString();
			}
			else if (b.Text.Equals("M-"))
			{
				memory -= Int32.Parse(MainScreen.Text);
			}
			else if (b.Text.Equals("M+"))
			{
				memory += Int32.Parse(MainScreen.Text);
			}

		}

		//=============================================================================================== Screen/Input window checks/resets/etc

		/*
		 * Checks if the input is valid to be in the int32 range
		 */
		private bool IntValidRange(int i)
		{
			return i >= -2147483648 && i <= 2147483647;

		}

		/*
		 * Checks if an investment percentage is in valid range (i.e. no one will offer you 100% returns)
		 */
		private bool ValidRate(Double d)
		{
			return d >= 0 && d <= 100;
		}

		//=============================================================================================== Functions which run computations

		/*
		 * Calculates the investment total, updating the display after completed
		 */
		private void Final_Clicked(object sender, EventArgs e)
		{
			// Parse all amounts into integers
			int start = Int32.Parse(StrAmt.Text.Substring(1));
			int years = Int32.Parse(YearsLabel.Text);
			double perc = Double.Parse(RateLabel.Text.Substring(0, RateLabel.Text.Length - 1)) / 100;
			int investment = Int32.Parse(InvestLabel.Text.Substring(1));
			int depositsPerYear;

			if (!IntValidRange(start) || !IntValidRange(years) || !ValidRate(perc) || !IntValidRange(investment))
			{
				MainScreen.Text = "Error: Invalid input";
			}
			else
			{
				if (FreqLabel.Text.Equals("Monthly"))
				{
					depositsPerYear = 12;
				}
				else if (FreqLabel.Text.Equals("Quarterly"))
				{
					depositsPerYear = 4;
				}
				else
				{
					depositsPerYear = 1;
				}
				FinalLabel.Text = "$" + Compute(start, years, perc, investment, depositsPerYear);
			}
		}

		/*
		* start is the starting balance
		* years is the number of years that the investment will encompass
		* perc is the annual rate of return (e.g., 6% should sent in as 0.06)
		* investment is the amount of money added to the account on a regular basis
		* depositsPerYear is the number times per year (12, 4, or 1) that a deposit is made.
		*/
		private int Compute(int start, int years, double perc, int investment, int depositsPerYear)
		{
			double bal = start;
			double monthlyRate = perc / 12.0;
			int monthsToDeposit = 12 / depositsPerYear;
			for (int y = 0; y < years; y++)
			{
				for (int m = 1; m <= 12; m++)
				{
					bal += bal * monthlyRate;
					if (m % monthsToDeposit == 0)
					{
						bal += investment;      // make deposits at the end of the month
					}
				}
			}
			return (int)Math.Round(bal);
		}

		/*
		 * Add operation. Will add the previous number to the next number in the input
		 */
		private void Operation_Clicked(object sender, EventArgs e)
		{
			Button caller = (Button)sender;

			if (caller == DivideButton)
			{
				// Clear out other stacks. Add the number to the new stack. Set top label text to 0 again (Same process for all)
				ClearAllOperationalStacks();
				divideStack.Push(Int32.Parse(MainScreen.Text));
				MainScreen.Text = "0";
			}
			else if (caller == MultiplyButton)
			{
				ClearAllOperationalStacks();
				multiplyStack.Push(Int32.Parse(MainScreen.Text));
				MainScreen.Text = "0";
			}
			else if (caller == AdditionButton)
			{
				ClearAllOperationalStacks();
				additionStack.Push(Int32.Parse(MainScreen.Text));
				MainScreen.Text = "0";
			}
			else if (caller == SubtractionButton)
			{
				ClearAllOperationalStacks();
				subtractionStack.Push(Int32.Parse(MainScreen.Text));
				MainScreen.Text = "0";
			}

		}

		/*
		 * Function which clears all numeric stacks in order to handle if the user
		 * clicks another operation
		 */
		private void ClearAllOperationalStacks()
		{
			additionStack.Clear();
			multiplyStack.Clear();
			divideStack.Clear();
			subtractionStack.Clear();
		}

		/*
		 * Handles when the '=' button is clicked, runs computation for mathematical operations
		 */
		private void Equals_Clicked(object sender, EventArgs e)
		{
			if (!(additionStack.Count() == 0))
			{
				int op1 = Int32.Parse(MainScreen.Text);
				MainScreen.Text = (additionStack.Pop() + op1).ToString();
				DisableIfNegative();
			}
			else if (!(divideStack.Count() == 0))
			{
				int op1 = Int32.Parse(MainScreen.Text);
				MainScreen.Text = (divideStack.Pop() / op1).ToString();
				DisableIfNegative();
			}
			else if (!(subtractionStack.Count() == 0))
			{
				int op1 = Int32.Parse(MainScreen.Text);
				MainScreen.Text = (subtractionStack.Pop() - op1).ToString();
				DisableIfNegative();
			}
			else if (!(multiplyStack.Count() == 0))
			{
				int op1 = Int32.Parse(MainScreen.Text);
				MainScreen.Text = (multiplyStack.Pop() * op1).ToString();
				DisableIfNegative();
			}
		}

		/*
		 * Checks if the screen contains negative values. If so, disables the investment buttons
		 */
		private void DisableIfNegative()
		{
			int num = Int32.Parse(MainScreen.Text);
			if (num < 0)
			{
				DisableEnableButtons(true);
			}
		}



		//=============================================================================================== Numeric click handlers (numbers, +/-)

		/*
		 * Handles the clicking of numeric buttons. 
		 * Does not allow 0 to be a leading digit.
		 * Enforces a length limitation so the screen does not resize itself.
		 */
		private void Numeric_Clicked(object sender, EventArgs e)
		{
			Button b = (Button)sender;

			if (!b.Text.Equals("0") && (MainScreen.Text.Equals("") || MainScreen.Text.Equals("0")))
			{
				MainScreen.Text = "";
				if (!b.Text.Equals("0") && MainScreen.Text.Length < 10) // Since 0 can't be a leading digit
				{
					MainScreen.Text += b.Text;
				}
			}
			else if (!MainScreen.Text.Equals("0") && MainScreen.Text.Length <= 10)
			{
				MainScreen.Text += b.Text;
			}
		}

		/*
		 * Handles the +/- clicker, will toggle positive/negative.
		 */
		private void PlusMinus_Clicked(object sender, EventArgs e)
		{
			if (!(MainScreen.Text.Equals("0")))
			{
				int previous = -1 * Int32.Parse(MainScreen.Text);
				MainScreen.Text = previous.ToString();

				// If the screen is now negative, all investment buttons must be disabled to prevent invalid input
				if (previous < 0)
				{
					DisableEnableButtons(true);
				}
				else
				{
					DisableEnableButtons(false);
				}
			}
		}

		/*
		 * If the screen's value is negative, we must disable the investment buttons. 
		 * If the screen's value is positie, we must re-enable the investment buttons.
		 */
		private void DisableEnableButtons(bool wantDisabled)
		{
			if (wantDisabled)
			{
				StartButton.IsEnabled = false;
				YearsButton.IsEnabled = false;
				RateButton.IsEnabled = false;
				InvestButton.IsEnabled = false;
			}
			else
			{
				StartButton.IsEnabled = true;
				YearsButton.IsEnabled = true;
				RateButton.IsEnabled = true;
				InvestButton.IsEnabled = true;
			}
		}

		//=============================================================================================== Data input click handlers (Starting, invest, freq, etc.)

		/*
		* Adds the desired starting amount to the display
		*/
		private void Start_Clicked(object sender, EventArgs e)
		{
			if ((!(MainScreen.Text == null || MainScreen.Text.Equals(""))))
			{
				StrAmt.Text = "$" + MainScreen.Text;
				MainScreen.Text = "";
			}
			MainScreen.Text = "0";
		}

		/*
		* Adds the desired years amount to the display
		*/
		private void Years_Clicked(object sender, EventArgs e)
		{
			if ((!(MainScreen.Text == null || MainScreen.Text.Equals(""))) && Int32.Parse(MainScreen.Text) > 0)
			{
				YearsLabel.Text = MainScreen.Text;
				MainScreen.Text = "";
			}
			MainScreen.Text = "0";
		}

		/*
		 * Adds the desired rate to the display
		 */
		private void Rate_Clicked(object sender, EventArgs e)
		{
			if ((!(MainScreen.Text == null || MainScreen.Text.Equals(""))))
			{
				RateLabel.Text = MainScreen.Text + "%";
				MainScreen.Text = "";
			}
			MainScreen.Text = "0";
		}

		/*
		 * Adds the desired amount of investment to the display
		 */
		private void Invest_Clicked(object sender, EventArgs e)
		{
			if ((!(MainScreen.Text == null || MainScreen.Text.Equals(""))))
			{
				InvestLabel.Text = "$" + MainScreen.Text;
				MainScreen.Text = "";
			}
			MainScreen.Text = "0";
		}

		/*
		 * Displays a picker to allow the user to pick a frequency of investment
		 */
		private async void Frequency_Clicked(object sender, EventArgs e)
		{
			string decision = await DisplayActionSheet("Frequency", "Cancel", null, "Monthly", "Quarterly", "Annually");
			if (!(decision == null) && !decision.Equals("Cancel"))
			{
				FreqLabel.Text = decision;
			}
		}

		//=============================================================================================== Clear display handler

		/*
		 * Resets the display to 0
		 */
		private void Clear_Clicked(object sender, EventArgs e)
		{
			MainScreen.Text = "0";
			// Re-enable investment buttons if they were disabled. If they weren't, not an issue.
			DisableEnableButtons(false);
		}


	}
}
