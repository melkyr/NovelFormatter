-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               11.7.2-MariaDB-ubu2404 - mariadb.org binary distribution
-- Server OS:                    debian-linux-gnu
-- HeidiSQL Version:             12.10.0.7000
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for noveldatabase
CREATE DATABASE IF NOT EXISTS `noveldatabase` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci */;
USE `noveldatabase`;

-- Dumping structure for table noveldatabase.chapters
CREATE TABLE IF NOT EXISTS `chapters` (
  `chapter_id` int(11) NOT NULL AUTO_INCREMENT,
  `novel_id` int(11) NOT NULL,
  `volume_id` int(11) DEFAULT NULL,
  `chapter_number` int(11) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `content_plain` text DEFAULT NULL,
  `content_bulma` text DEFAULT NULL,
  `content_html` text DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`chapter_id`),
  KEY `novel_id` (`novel_id`),
  KEY `volume_id` (`volume_id`),
  CONSTRAINT `chapters_ibfk_1` FOREIGN KEY (`novel_id`) REFERENCES `novels` (`novel_id`),
  CONSTRAINT `chapters_ibfk_2` FOREIGN KEY (`volume_id`) REFERENCES `volumes` (`volume_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Data exporting was unselected.

-- Dumping structure for table noveldatabase.novels
CREATE TABLE IF NOT EXISTS `novels` (
  `novel_id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `author` varchar(255) NOT NULL,
  `status` enum('ongoing','finished','hiatus') NOT NULL,
  `description` text DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`novel_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Data exporting was unselected.

-- Dumping structure for table noveldatabase.volumes
CREATE TABLE IF NOT EXISTS `volumes` (
  `volume_id` int(11) NOT NULL AUTO_INCREMENT,
  `novel_id` int(11) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `volume_number` int(11) DEFAULT NULL,
  `description` text DEFAULT NULL,
  PRIMARY KEY (`volume_id`),
  KEY `novel_id` (`novel_id`),
  CONSTRAINT `volumes_ibfk_1` FOREIGN KEY (`novel_id`) REFERENCES `novels` (`novel_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Data exporting was unselected.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
