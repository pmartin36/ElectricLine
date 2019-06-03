using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
	AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class InspectorReadOnlyAttribute: PropertyAttribute {
	public bool ReadOnly;

	public InspectorReadOnlyAttribute(bool readOnly) {
		ReadOnly = readOnly;
	}

	public InspectorReadOnlyAttribute() : this(true) { }
}

