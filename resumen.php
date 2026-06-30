<?php
session_start();

if (isset($_GET['cerrar'])) {
    session_destroy();
    header("Location: ingreso.html");
    exit();
}

if (!isset($_SESSION['documento'])) {
    header("Location: ingreso.html");
    exit();
}

$host = 'localhost';
$db   = 'mi_banco_db';
$user = 'root';
$pass = '';

$conn = new mysqli($host, $user, $pass, $db);

if ($conn->connect_error) {
    die("Conexión fallida: " . $conn->connect_error);
}

$documento = $_SESSION['documento'];

$sql = "SELECT u.nombre, u.apellido, u.usuario,
               t.numero_tarjeta, t.banco_emisor, t.estado, t.saldo, t.num_cuenta
        FROM   usuarios u
        INNER  JOIN tarjetas t ON t.dni_titular = u.documento
        WHERE  u.documento = ?";

$stmt = $conn->prepare($sql);
$stmt->bind_param("s", $documento);
$stmt->execute();
$stmt->store_result();
$stmt->bind_result($nombre, $apellido, $usuario, $numero_tarjeta, $banco_emisor, $estado, $saldo, $num_cuenta);
$stmt->fetch();
$stmt->close();

$sql2 = "SELECT periodo, fecha_vencimiento, total_a_pagar, pago_minimo
         FROM   liquidaciones
         WHERE  num_cuenta = ?
         ORDER  BY periodo DESC";

$stmt2 = $conn->prepare($sql2);
$stmt2->bind_param("i", $num_cuenta);
$stmt2->execute();
$stmt2->store_result();
$stmt2->bind_result($periodo, $fecha_venc, $total, $minimo);

$liquidaciones = [];
while ($stmt2->fetch()) {
    $liquidaciones[] = [
        'periodo'   => $periodo,
        'vencimiento' => $fecha_venc,
        'total'     => $total,
        'minimo'    => $minimo,
    ];
}
$stmt2->close();
$conn->close();

$ultima      = $liquidaciones[0] ?? null;
$historial   = array_slice($liquidaciones, 1);
$ultimos4    = substr($numero_tarjeta ?? '', -4);
$badge_color = ($estado ?? '') === 'Activa' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700';
?>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mis Tarjetas - Resumen</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-gray-100 font-sans min-h-screen flex flex-col">

    <header class="bg-[#004691] text-white py-4 shadow-md px-6 flex items-center justify-between">
        <h1 class="text-xl font-semibold">Mis <span class="font-bold">Tarjetas</span></h1>
        <div class="flex items-center gap-4 text-sm">
            <span>Hola, <strong><?= htmlspecialchars($nombre ?? '') ?></strong></span>
            <a href="resumen.php?cerrar=1" class="bg-white text-[#004691] font-semibold px-4 py-1 rounded-full hover:bg-gray-100 transition">Cerrar sesión</a>
        </div>
    </header>

    <main class="flex-grow max-w-4xl mx-auto w-full p-6 space-y-6">

        <!-- Datos de la tarjeta -->
        <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-xs font-semibold text-gray-400 uppercase mb-4">Tu tarjeta</h2>
            <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div>
                    <p class="text-2xl font-bold text-gray-800 tracking-widest">**** **** **** <?= htmlspecialchars($ultimos4) ?></p>
                    <p class="text-sm text-gray-500 mt-1"><?= htmlspecialchars($banco_emisor ?? '') ?> &mdash; <?= htmlspecialchars($usuario ?? '') ?></p>
                </div>
                <div class="flex items-center gap-3">
                    <span class="text-xs font-semibold px-3 py-1 rounded-full <?= $badge_color ?>">
                        <?= htmlspecialchars($estado ?? '') ?>
                    </span>
                    <div class="text-right">
                        <p class="text-xs text-gray-400">Saldo disponible</p>
                        <p class="text-xl font-bold text-[#004691]">$ <?= number_format($saldo ?? 0, 2, ',', '.') ?></p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Última liquidación -->
        <?php if ($ultima): ?>
        <div class="bg-[#004691] text-white rounded-lg shadow p-6">
            <h2 class="text-xs font-semibold uppercase opacity-70 mb-4">Último resumen — <?= htmlspecialchars($ultima['periodo'] ?? '') ?></h2>
            <div class="grid grid-cols-2 gap-6">
                <div>
                    <p class="text-xs opacity-70">Total a pagar</p>
                    <p class="text-3xl font-bold mt-1">$ <?= number_format($ultima['total'] ?? 0, 2, ',', '.') ?></p>
                </div>
                <div>
                    <p class="text-xs opacity-70">Pago mínimo</p>
                    <p class="text-3xl font-bold mt-1">$ <?= number_format($ultima['minimo'] ?? 0, 2, ',', '.') ?></p>
                </div>
            </div>
            <p class="text-xs opacity-70 mt-4">Vencimiento: <strong><?= htmlspecialchars($ultima['vencimiento'] ?? '') ?></strong></p>
        </div>
        <?php else: ?>
        <div class="bg-white rounded-lg shadow p-6 text-center text-gray-500">
            No hay liquidaciones registradas para tu tarjeta.
        </div>
        <?php endif; ?>

        <!-- Historial -->
        <?php if (!empty($historial)): ?>
        <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-xs font-semibold text-gray-400 uppercase mb-4">Historial de resúmenes</h2>
            <table class="w-full text-sm text-left">
                <thead>
                    <tr class="text-xs text-gray-400 uppercase border-b border-gray-100">
                        <th class="pb-2">Período</th>
                        <th class="pb-2">Vencimiento</th>
                        <th class="pb-2 text-right">Total</th>
                        <th class="pb-2 text-right">Pago mínimo</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-50">
                    <?php foreach ($historial as $liq): ?>
                    <tr class="hover:bg-gray-50">
                        <td class="py-3 font-medium text-gray-700"><?= htmlspecialchars($liq['periodo'] ?? '') ?></td>
                        <td class="py-3 text-gray-500"><?= htmlspecialchars($liq['vencimiento'] ?? '') ?></td>
                        <td class="py-3 text-right text-gray-800 font-semibold">$ <?= number_format($liq['total'] ?? 0, 2, ',', '.') ?></td>
                        <td class="py-3 text-right text-gray-500">$ <?= number_format($liq['minimo'] ?? 0, 2, ',', '.') ?></td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        </div>
        <?php endif; ?>

    </main>

    <footer class="bg-gray-50 text-[10px] text-gray-500 text-center p-4 border-t border-gray-200">
        Portal Oficial de Consultas de Liquidaciones Progra3card.
    </footer>
</body>
</html>
