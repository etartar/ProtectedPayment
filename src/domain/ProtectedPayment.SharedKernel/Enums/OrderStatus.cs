namespace ProtectedPayment.SharedKernel.Enums;

public enum OrderStatus
{
    Pending, // Provizyonda bekliyor (10 dakikalık süre)
    Confirmed, // 10 dakika doldu, ödeme onaylandı
    CancelRequested, // İptal talebi geldi ama henüz işlenmedi
    Cancelled, // İptal edildi, iade yapıldı
    Shipped
}
