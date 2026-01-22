namespace ProtectedPayment.Database.Entities;

public enum OrderStatus
{
    Pending, // Provizyonda bekliyor (10 dakikalık süre)
    Confirmed, // 10 dakika doldu, ödeme onaylandı
    Cancelled, // İptal edildi, iade yapıldı
    Shipped
}
