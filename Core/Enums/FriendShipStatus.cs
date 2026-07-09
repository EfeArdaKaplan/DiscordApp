using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Enums
{
    public enum FriendShipStatus
    {
        Pending = 1,  // istek atıldı, onay bekleniyor
        Accepted = 2, // istek kabul edildi (Arkadaştırlar)
        Rejected = 3, // istek reddedildi
    }
}
