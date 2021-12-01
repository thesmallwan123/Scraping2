-- phpMyAdmin SQL Dump
-- version 4.7.4
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Gegenereerd op: 01 dec 2021 om 10:13
-- Serverversie: 10.1.30-MariaDB
-- PHP-versie: 7.2.1

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `motie`
--

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `motie`
--

DROP TABLE IF EXISTS `motie`;
CREATE TABLE `motie` (
  `id` varchar(11) NOT NULL,
  `did` varchar(11) NOT NULL,
  `title` text NOT NULL,
  `omschrijving` text NOT NULL,
  `stemmenVoor` int(3) NOT NULL,
  `motieDatum` varchar(10) NOT NULL DEFAULT '0000-00-00',
  `partijVoor` varchar(256) DEFAULT NULL,
  `partijTegen` varchar(256) DEFAULT NULL,
  `geld` int(11) NOT NULL DEFAULT '0',
  `CO2Waarde` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Indexen voor geëxporteerde tabellen
--

--
-- Indexen voor tabel `motie`
--
ALTER TABLE `motie`
  ADD PRIMARY KEY (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
