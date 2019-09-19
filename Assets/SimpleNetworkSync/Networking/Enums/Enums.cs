using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Networking
{
	public enum Realm { Primary = 1, Ghost = 2, Both = 3 }

	public enum Replication { OwnerSend = 1, MasterSend = 2 }
	public enum Interpolation { None, Linear, CatmullRom }
}
