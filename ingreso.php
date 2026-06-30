<?php
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    die("Acceso no permitido.");
}

$host = 'localhost';
$db   = 'mi_banco_db';
$user = 'root';
$pass = '';

$conn = new mysqli($host, $user, $pass, $db);

if ($conn->connect_error) {
    die("Conexión fallida: " . $conn->connect_error);
}

$usuario   = trim($_POST['usuario']   ?? '');
$tipo_doc  = trim($_POST['tipo_doc']  ?? '');
$documento = trim($_POST['documento'] ?? '');
$password  = trim($_POST['password']  ?? '');

$sql = "SELECT usuario,tipo_doc,documento,nombre,apellido
        FROM usuarios
        WHERE usuario = ? AND tipo_doc = ? AND documento = ? AND password = ?";

$stmt = $conn->prepare($sql);
$stmt->bind_param("ssss", $usuario, $tipo_doc, $documento, $password);

$stmt->execute();
$stmt->store_result();

if ($stmt->num_rows > 0) {
    $stmt->bind_result($usuario, $tipo_doc, $documento,$nombre,$apellido);
    $stmt->fetch();

    session_start();
    
    $_SESSION['usuario']    = $usuario;
    $_SESSION['tipo_doc']   = $tipo_doc;
    $_SESSION['documento']  = $documento;
    $_SESSION['nombre']  = $nombre;
    $_SESSION['apellido']  = $apellido;
    

    header("Location: resumen.php");
    exit();
} else {
    echo "<script>alert('Usuario o contraseña incorrectos.'); window.location='ingreso.html';</script>";
}

$stmt->close();
$conn->close();
?>
