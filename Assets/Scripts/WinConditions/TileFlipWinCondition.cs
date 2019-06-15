using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TileFlipCondition : WinCondition {
	public TileFlipCondition(int total, int depth) {
		Total = total;
		Current = 0;

		//TODO: Goal based on depth
		Target = total - 2;
	}
}

