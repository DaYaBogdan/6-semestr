exp_table <- read_excel("ExpData.xlsx")
exp_table

par(font.lab=3, cex.lab=1.5, font.main=4, cex.main=2)
par(lty=2)
par(pch=17)
plot(exp_table$Убийства, exp_table$Индекс_счастья)
# dev.new()
# plot(exp_table$Убийства, exp_table$ИПЦ)
# dev.new()
# plot(exp_table$Безработица_25_54, exp_table$Инфляция)