<?php 
$host = 'localhost';
$db   = 'mi_banco_db';
$user = 'root';
$pass = '';


$pdo = new PDO("mysql:host=$host;dbname=$db", $user, $pass);
$pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

$documento = trim($_POST['document'] ?? '');
$tipo_doc  = $_POST['tipo_doc']       ?? '';
$nombre    = trim($_POST['nombre']    ?? '');
$apellido  = trim($_POST['apellido']  ?? '');
$usuario   = trim($_POST['usuario']   ?? '');
$passA     = $_POST['passwordA']      ?? '';
$passB     = $_POST['passwordB']      ?? '';














?>