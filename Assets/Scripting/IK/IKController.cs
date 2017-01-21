using UnityEngine;
using System.Collections;

public interface IKController {
	bool AllowControl(IKController ikc,IKRig ikr);
}
