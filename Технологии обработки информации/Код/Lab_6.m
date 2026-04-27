% Параметры
f_s = 100; 
n_soob = 4096;
delta_f = f_s / (n_soob - 1);
f = 0 : delta_f : f_s;
nf = length(f);

% Частоты среза (по визуальной оценке ПСМ)
fc_nch = 12;    % ФНЧ: конец области роста
fc_vch = 40;    % ФВЧ: начало высокочастотной области (до Найквиста)

% Индексы среза
K_nch = round(fc_nch / delta_f) + 1;
K_vch = round(fc_vch / delta_f) + 1;   % теперь <= K_nyq

% Маски фильтров (полные, симметричные 0..f_s)
H_nc = zeros(1, nf);
H_nc(1:K_nch) = 1.0;

H_vc = ones(1, nf);
H_vc(1:K_vch) = 0.0;                  % обнуляем до 45 Гц

% ---- Подготовка к yulewalk ----
K_nyq = round((f_s/2) / delta_f) + 1;   % 2049

% Берём только до Найквиста
f_half = f(1:K_nyq);
H_nc_half = H_nc(1:K_nyq);
H_vc_half = H_vc(1:K_nyq);

% Нормировка частот
f_norm = f_half / (f_s/2);
f_norm(1) = 0;
f_norm(end) = 1;

% Синтез
order = 8;
[b_nc, a_nc] = yulewalk(order, f_norm, H_nc_half);
[b_vc, a_vc] = yulewalk(order, f_norm, H_vc_half);

% ЧХ синтезированного фильтра для контроля
[h1,w1]=freqz(b_nc,a_nc,128);
[h2,w2]=freqz(b_vc,a_vc,128);

%%
% Частотные точки (нормированные)
f_p = [0, 2.5/50, 3/50, 12/50, 13/50, 1];
H_p = [0, 0, 1, 1, 0, 0];

f_z = [0, 2.5/50, 3/50, 12/50, 13/50, 1];
H_z = [1, 1, 0, 0, 1, 1];
%%
order = 100;

b3 = fir2(order, f_p, H_p);
b4 = fir2(order, f_z, H_z);

% контрольный расчёт получившегося
[h3,w3] = freqz(b3,1.0,128);
[h4,w4] = freqz(b4,1.0,128);

%%

figure(5)

subplot (2,2,1)
plot(f_norm, H_nc_half,'-r','LineWidth',2);
hold;
plot(w1/pi, abs(h1),'--g','LineWidth',2);
grid on;
title('Frequency responses');
ylabel('\it H(\omega / \pi )');
legend('Dreams','Result','Location','best');

subplot (2,2,2)
plot(f_norm, H_vc_half,'-r','LineWidth',2);
hold;
plot(w2/pi, abs(h2),'--g','LineWidth',2);
grid on;
title('Frequency responses');
ylabel('\it H(\omega / \pi )');
legend('Dr...','Re...', 'Location','best');

subplot (2,2,3)
plot(f_p,H_p,'-r','LineWidth',2);
hold;
plot(w3/pi,abs(h3),'--g','LineWidth',2);
grid on;
xlabel('Normalized Frequency');
ylabel('\it H(\omega / \pi )');
legend('Ideal','Design','Location','best')

subplot (2,2,4)
plot(f_z,H_z,'-r','LineWidth',2);
hold;
plot(w4/pi,abs(h4),'--g','LineWidth',2);
grid on;
xlabel('Normalized Frequency');
ylabel('\it H(\omega / \pi )');
legend('Ideal','Design', 'Location','best')

%% -------- ПОДГОТОВКА ИСХОДНЫХ СИГНАЛОВ (Lab_4) -------- %%
% Если model_2 уже есть в workspace, этот блок можно пропустить.
% Иначе воспроизводим генерацию model_2 из Lab_4.
if ~exist('model_2','var')
    z = [-1.65];
    p = [-2.1 -3.41];
    % k = 1; (не используется)
    T = 0.01;
    denominator = (1+5.51*T+7.161*T*T);
    zd = [(1+1.65*T)/denominator -1/denominator];
    pd = [-(2+11.02*T)/denominator 1/denominator];
    [b1, a1] = eqtflength(zd, pd);
    n_soob = 4096;
    s_v = rand(1, n_soob);
    model_2 = filter(b1, a1, s_v);
end

% При необходимости обработайте и model_1 (НЦФ).
% if ~exist('model_1','var')
%     h = [-0.48 0.24 0.11 0.055 0.026 0.012];
%     m_akf = 41;
%     h_padded = [h(:); zeros(m_akf - length(h), 1)];
%     model_1 = filter(h_padded, 1.0, s_v);
% end

%% -------- ФИЛЬТРАЦИЯ СИГНАЛА -------- %%
% Обрабатываем model_2 (можно заменить на model_1 для сравнения)
signal = model_2;

% БИХ фильтры (yulewalk)
y_nc = filter(b_nc, a_nc, signal);
y_vc = filter(b_vc, a_vc, signal);

% КИХ фильтры (fir2)
y_bp = filter(b3, 1, signal);   % полосовой
y_bs = filter(b4, 1, signal);   % режекторный

%% -------- БПФ И ПОДГОТОВКА ОСЕЙ -------- %%
% Спектры выходных сигналов (симметричные)
Y_nc = fftshift(fft(y_nc));
Y_vc = fftshift(fft(y_vc));
Y_bp = fftshift(fft(y_bp));
Y_bs = fftshift(fft(y_bs));

% Ось частот для симметричного представления
f_sym = -f_s/2 : delta_f : f_s/2;   % длина 4096, если f_s/2 точно 50, то ровно 4096

% Модули спектров (для наглядности можно в дБ: 20*log10(abs(...)))
A_nc = abs(Y_nc);
A_vc = abs(Y_vc);
A_bp = abs(Y_bp);
A_bs = abs(Y_bs);

%% -------- ПОСТРОЕНИЕ ЖЕЛАЕМЫХ МАСОК ДЛЯ СИММЕТРИЧНОГО ДИАПАЗОНА -------- %%
% Маска ФНЧ: 0..12 Гц пропускаем, 13..50 и -50..-12 подавляем
% Проще: создаём симметричную маску, где желаемое усиление = H_nc (для f>=0) и зеркально для f<0
H_nc_sym = [fliplr(H_nc(2:end)), H_nc];   % длина 2*4096-1 = 8191 ??? Нет, у нас H_nc длина 4096 (0..f_s).
% Но для оси f_sym (от -f_s/2 до f_s/2) нужно брать значения, соответствующие частотам.
% Лучше: определить маску непосредственно на сетке f_sym, используя желаемые интервалы.
% Так как у нас f_sym идёт от -50 до 50 с шагом delta_f, а f_asi от 0 до 100.
% Индексы: положительные частоты f_sym>=0 соответствуют f_asi, отрицательные – дополняются зеркально.

% Создаём маски фильтров на всей оси f_sym (длина 4096)
H_mask_nc = zeros(size(f_sym));   % заготовка
H_mask_vc = ones(size(f_sym));    % ФВЧ – изначально 1
H_mask_bp = zeros(size(f_sym));
H_mask_bs = ones(size(f_sym));

% Интервалы в Гц
f1 = 0; f2 = 12;        % для ФНЧ и полосового (нижняя 0, верхняя 12)
f3 = 3; f4 = 12;        % границы полосы/режекции
f5 = 40;                % нижняя для ФВЧ

% Заполняем маски
% ФНЧ: пропускаем от -12 до 12 Гц
H_mask_nc(abs(f_sym) <= 12) = 1;
% ФВЧ: подавляем от -40 до 40 Гц
H_mask_vc(abs(f_sym) <= 40) = 0;
% Полосовой: пропускаем от 3 до 12 Гц и от -12 до -3 Гц
H_mask_bp((abs(f_sym) >= 3) & (abs(f_sym) <= 12)) = 1;
% Режекторный: подавляем от 3 до 12 Гц и от -12 до -3 Гц
H_mask_bs((abs(f_sym) >= 3) & (abs(f_sym) <= 12)) = 0;

% Для наложения на график можно масштабировать маску до максимума спектра,
% чтобы она была видна. Обычно её рисуют отдельной кривой, а не накладывают
% с амплитудой. Достаточно построить её как линию с отдельной осью или
% перенормировать к уровню максимума спектра.

%% -------- ВИЗУАЛИЗАЦИЯ СПЕКТРОВ С НАЛОЖЕННЫМИ ЧХ -------- %%
figure('Name', 'Спектры обработанных сигналов и желаемые ЧХ');

% 1. ФНЧ
subplot(2,2,1)
plot(f_sym, A_nc, 'b', 'LineWidth', 1.5);
hold on;
% Нормируем маску к максимуму спектра для наглядного совмещения
yyaxis right;
plot(f_sym, H_mask_nc, 'r--', 'LineWidth', 2);
ylabel('H_{ideal}');
yyaxis left;
grid on;
xlabel('Частота, Гц');
ylabel('|Y_{out}(f)|');
title('ФНЧ — выходной спектр');
legend('Спектр','Желаемая ЧХ','Location','best');
xlim([-50 50]);

% 2. ФВЧ
subplot(2,2,2)
plot(f_sym, A_vc, 'b', 'LineWidth', 1.5);
hold on;
yyaxis right;
plot(f_sym, H_mask_vc, 'r--', 'LineWidth', 2);
yyaxis left;
grid on;
xlabel('Частота, Гц');
title('ФВЧ — выходной спектр');
xlim([-50 50]);

% 3. Полосовой
subplot(2,2,3)
plot(f_sym, A_bp, 'b', 'LineWidth', 1.5);
hold on;
yyaxis right;
plot(f_sym, H_mask_bp, 'r--', 'LineWidth', 2);
yyaxis left;
grid on;
xlabel('Частота, Гц');
title('Полосовой (3–12 Гц)');
xlim([-50 50]);

% 4. Режекторный
subplot(2,2,4)
plot(f_sym, A_bs, 'b', 'LineWidth', 1.5);
hold on;
yyaxis right;
plot(f_sym, H_mask_bs, 'r--', 'LineWidth', 2);
yyaxis left;
grid on;
xlabel('Частота, Гц');
title('Режекторный (3–12 Гц)');
xlim([-50 50]);