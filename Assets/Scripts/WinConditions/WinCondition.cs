using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class WinCondition {
	public int Target { get; set; }
	public int Total { get; set; }
	public int Current { get; set; }
	public bool IsTargetReached { get => Current >= Target; }

	public void IncrementCurrent() {
		Current++;
	}
}

