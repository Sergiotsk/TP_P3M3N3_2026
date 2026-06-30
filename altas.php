<?php 
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    die("Acceso no permitido.");
}

$host = 'localhost';
$db   = 'mi_banco_db';
$user = 'root';
$pass = '';

$conn = new mysqli($host,$user,$pass,$db);

if ($conn->connect_error) {
    die("Conexión fallida: " . $conn->connect_error);
}

$documento = trim($_POST['documento'] ?? '');
$tipo_doc  = trim($_POST['tipo_doc']       ?? '');
$nombre    = trim($_POST['nombre']    ?? '');
$apellido  = trim($_POST['apellido']  ?? '');
$usuario   = trim($_POST['usuario']   ?? '');
$passA     = trim($_POST['passwordA']      ?? '');
$passB     = trim($_POST['passwordB']      ?? '');

$sql= "SELECT dni_titular
       FROM tarjetas 
       WHERE dni_titular = ?";


$stmt = $conn->prepare($sql);
$stmt->bind_param("s", $documento);

$stmt->execute();
$stmt->store_result();

if ($stmt->num_rows > 0){

if ($passA !== $passB) {
    die("Error: las contraseñas no coinciden.");
}

$sql= "UPDATE usuarios
       SET  usuario = ?,password = ?
       WHERE documento = ?";

$stmt = $conn->prepare($sql);
$stmt->bind_param("sss", $usuario,$passA,$documento);

if ($stmt->execute()) {
    echo "<script>alert('Usuario activado correctamente.'); window.location='ingreso.html';</script>";
} else {
    echo "Error: " . $stmt->error;
}


}else{
    die("Cliente inexistente");
}




$stmt->close();
$conn->close();










?>