x <- c(1:10) # Создание x из массива от 1 до 10
y <- x	 # Присваивание из х массива в y
z <- 10/x	 # Деление 10 на каждый элемент
opar <- par(no.readonly=TRUE)	# Возврат параметров, чтобы вновь использовать
par(mar=c(5, 4, 4, 8) + 0.1)	# Задание отступов
plot(x, y, type="b", pch=21, col="red", yaxt="n", lty=3, ann=FALSE) # отрисовка красной прямой
lines(x, z, type="b", pch=22, col="blue", lty=2) # Отрисовка зависимости x от z
axis(2, at=x, labels=x, col.axis="red", las=2) # Отрисовка оси для x
axis(4, at=z, labels=round(z, digits=2), col.axis="blue", las=2, cex.axis=0.7, tck=-.01) # Отрисовка оси для z
mtext("y=1/x", side=4, line=3, cex.lab=1, las=2, col="blue") # Размещение подзаголовка
title("Пример осей", xlab="значение переменной X", ylab="Y=X") # Размещение заголовка для графиков
par(opar) # Задание стандартных параметров 