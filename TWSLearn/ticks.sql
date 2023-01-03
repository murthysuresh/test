SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL';

DROP SCHEMA IF EXISTS `tick_db` ;
CREATE SCHEMA IF NOT EXISTS `tick_db` DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci ;
USE `tick_db` ;

-- -----------------------------------------------------
-- Table `ticks`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `ticks` ;

CREATE  TABLE IF NOT EXISTS `ticks` (
  `idticks` INT NOT NULL ,
  `symbol` VARCHAR(8) NOT NULL ,
  `date` DATE NOT NULL  ,
  `time` DECIMAL(17,10) NOT NULL  ,
  `value` FLOAT NOT NULL ,
  `type` VARCHAR(12) NOT NULL ,
  PRIMARY KEY (`idticks`, `date`) )
ENGINE = InnoDB PARTITION BY KEY(date) PARTITIONS 1;

CREATE INDEX `Symbol` ON `ticks` (`symbol` ASC) ;



SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
