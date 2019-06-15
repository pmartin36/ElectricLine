using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EnemyWinCondition : WinCondition {
	public EnemyWinCondition(int depth) {
		Total = 6;
		Current = 0;

		//TODO: Goal based on depth
		Target = Total - 2;
	}
}

