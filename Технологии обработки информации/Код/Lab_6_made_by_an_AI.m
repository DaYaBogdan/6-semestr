%% -------- ИСХОДНЫЕ ДАННЫЕ -------- %%
z = [-1.65];
p = [-2.1 -3.41];
k = 1;

T = 0.01;

h = [-0.48 0.24 0.11 0.055 0.026 0.012];

m_akf = 41;
h_padded = [h(:); zeros(m_akf - length(h), 1)];

%% -------- ГЕНЕРАЦИЯ ПОСЛЕДОВАТЕЛЬНОСТИ ПСЕВДОСЛУЧАЙНЫХ -------- %%
n_soob = 4096;
s_v = rand(1, n_soob);

%% -------- РАСЧЁТ КОЭФФИЦИЕНТОВ -------- %%
denominator = (1 + 5.51*T + 7.161*T*T);
zd = [(1+1.65*T)/denominator, -1/denominator];
pd = [-(2+11.02*T)/denominator, 1/denominator];

[b1, a1] = eqtflength(zd, pd);
model_2 = filter(b1, a1, s_v);
[c_akf, lags] = xcorr(model_2, m_akf, 'coeff');

%% -------- ПОСТРОЕНИЕ ДИСКРЕТНЫХ ФУНКЦИЙ ПСМ -------- %%
g_asi = fft(c_akf);
g_sim = fftshift(g_asi);

% График (исправлено предупреждение о мнимых частях)
figure;
plot(lags, abs(g_sim), '-w', 'LineWidth', 2)
xlabel('Lags')
ylabel('|G_{sim}|')
title('Спектральная плотность мощности (модуль)')
grid on

%% ---- ПОДГОТОВКА ДАННЫХ ДЛЯ СИНТЕЗА ФИЛЬТРОВ ---- %%
% Частота дискретизации
Fs = 1/T;          % 100 Гц
Fnyq = Fs/2;       % частота Найквиста 50 Гц

% После fftshift нулевая частота находится в центре.
% Длина вектора g_sim равна 2*m_akf-1 = 81 (нечётная).
% Индекс центра: (81+1)/2 = 41.
N = length(g_sim);
center_idx = (N+1)/2;

% Берём только неотрицательные частоты (правая половина спектра)
g_right = g_sim(center_idx:end);   % от DC до Найквиста
M = length(g_right);               % 41

% Нормированные частоты для fir2 / yulewalk (диапазон [0, 1])
f_norm = (0:M-1) / (M-1);          % 0 : 1/40 : 1

% Амплитудная характеристика – модуль спектра
amp = abs(g_right);

%% ---- СИНТЕЗ КИХ‑ФИЛЬТРА С ПОМОЩЬЮ fir2 ---- %%
nf = 11;   % порядок фильтра (можно варьировать)

% Синтез КИХ‑фильтра с желаемой АЧХ = amp
b3 = fir2(nf, f_norm, amp);

% Контрольный расчёт АЧХ синтезированного фильтра
[h3, w3] = freqz(b3, 1, 128);

% Визуализация
figure;
plot(f_norm, amp, 'b-', 'LineWidth', 2); hold on
plot(w3/pi, abs(h3), 'r--', 'LineWidth', 2);
xlabel('Нормированная частота (\times \pi рад/отсчёт)')
ylabel('Амплитуда')
title('Синтез КИХ‑фильтра (fir2)')
legend('Желаемая АЧХ (по g_{sim})', 'Реализованная АЧХ', 'Location','best')
grid on

%% ---- СИНТЕЗ БИХ‑ФИЛЬТРА С ПОМОЩЬЮ yulewalk ---- %%
% Используем те же f_norm и amp.
% При необходимости можно сгладить или модифицировать amp,
% но здесь оставляем как есть.

[b_yw, a_yw] = yulewalk(nf, f_norm, amp);

% Контрольная АЧХ
[h_yw, w_yw] = freqz(b_yw, a_yw, 128);

figure;
plot(f_norm, amp, 'b-', 'LineWidth', 2); hold on
plot(w_yw/pi, abs(h_yw), 'g--', 'LineWidth', 2);
xlabel('Нормированная частота (\times \pi рад/отсчёт)')
ylabel('Амплитуда')
title('Синтез БИХ‑фильтра (yulewalk)')
legend('Желаемая АЧХ (по g_{sim})', 'Реализованная АЧХ', 'Location','best')
grid on

%%

% Приводим векторы к столбцам для единообразия
f_norm = f_norm(:);   % гарантированно столбец
amp = amp(:);         % гарантированно столбец

% Параметры БПФ
Nfft = 1024;
output_signal = filter(b3, 1, model_2);   % результат обработки

% Спектр выходного сигнала
Y = fft(output_signal, Nfft);
Y_shift = fftshift(Y);
f_plot = (-Nfft/2 : Nfft/2-1).' / Nfft;   % столбец частот от -0.5 до 0.5

% Создаём двустороннюю желаемую характеристику
% Положительные частоты (0..1) уже есть в f_norm и amp
% Для отрицательных частот зеркально отображаем amp (кроме DC)
H_desired_neg = flip(amp(2:end));          % зеркально для отрицательных частот
H_desired_double = [H_desired_neg; amp];   % объединение
f_desired_double = [-flip(f_norm(2:end)); f_norm]; % частоты для двусторонней характеристики

% Построение графика
figure;
plot(f_plot, abs(Y_shift), 'b', 'LineWidth', 1.5); hold on;
plot(f_desired_double, H_desired_double, 'r--', 'LineWidth', 2);
xlim([-0.5 0.5]);
xlabel('Нормированная частота');
ylabel('Амплитуда');
legend('Спектр выходного сигнала', 'Желаемая ЧХ фильтра');
title('Сравнение спектра обработанного сигнала и желаемой АЧХ');
grid on;

%% -------- ТЕСТИРОВАНИЕ НА СИНУСОИДЕ -------- %%
% Генерация синусоиды
Fs = 1/T;
t = (0:n_soob-1) * T;
f_sine = 5;            % Частота 5 Гц (норм. 0.1)
sine_wave = sin(2*pi*f_sine*t) + 0.01*randn(size(t)); % слабый шум

% Обработка фильтрами
out_fir = filter(b3, 1, sine_wave);
out_iir = filter(b_yw, a_yw, sine_wave);

% БПФ
Nfft = 1024;
Y_in = fftshift(fft(sine_wave, Nfft));
Y_fir = fftshift(fft(out_fir, Nfft));
Y_iir = fftshift(fft(out_iir, Nfft));
f_plot = (-Nfft/2:Nfft/2-1)/Nfft;

% Желаемая АЧХ (двусторонняя)
f_norm = f_norm(:); amp = amp(:);
H_neg = flip(amp(2:end));
H_dbl = [H_neg; amp];
f_dbl = [-flip(f_norm(2:end)); f_norm];

% Графики
figure('Name', 'Тест на синусоиде');
subplot(2,2,1);
plot(f_plot, abs(Y_in)); title('Спектр входной синусоиды'); xlim([-0.5 0.5]); grid on;

subplot(2,2,2);
plot(f_plot, abs(Y_fir)); hold on;
plot(f_dbl, H_dbl*max(abs(Y_fir)), 'r--');
title('Выход КИХ‑фильтра'); xlim([-0.5 0.5]); grid on; legend('Спектр','Желаемая АЧХ');

subplot(2,2,3);
plot(f_plot, abs(Y_iir)); hold on;
plot(f_dbl, H_dbl*max(abs(Y_iir)), 'g--');
title('Выход БИХ‑фильтра'); xlim([-0.5 0.5]); grid on; legend('Спектр','Желаемая АЧХ');

subplot(2,2,4);
plot(f_norm, amp, '-b', 'LineWidth',2); hold on;
[h_fir, w] = freqz(b3,1); plot(w/pi, abs(h_fir), 'r--');
[h_iir, w] = freqz(b_yw,a_yw); plot(w/pi, abs(h_iir), 'g--');
title('Сравнение АЧХ фильтров'); xlabel('Норм. частота'); legend('Желаемая','КИХ','БИХ'); grid on;