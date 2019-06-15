using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SwitchWinCondition : WinCondition {
	public SwitchWinCondition(int depth) {
		//TODO: Total based on depth
		Total = 4;
		Current = 0;

		//TODO: Goal based on depth
		Target = Total - 2;
	}
}

