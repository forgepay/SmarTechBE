using CryptoOnRamp.DAL.Models;
using System.Globalization;

namespace CryptoOnRamp.BLL.Localization;

public static class I18n
{
    public static readonly Dictionary<string, Dictionary<AppLanguage, string>> Text = new()
    {
        // ── Step 1: Language ─────────────────────────────────────────────
        ["choose_language"] = new()
    {
        { AppLanguage.English, "🌍 Please choose your language:" },
        { AppLanguage.Russian, "🌍 Пожалуйста, выберите язык:" },
        { AppLanguage.Hebrew, "🌍 אנא בחר/י שפה:" },
        { AppLanguage.Portuguese, "🌍 Por favor, escolha seu idioma:" }
    },
        ["btn_language_en"] = new()
    {
        { AppLanguage.English, "English" }, { AppLanguage.Russian, "English" },
        { AppLanguage.Hebrew, "English" }, { AppLanguage.Portuguese, "English" }
    },
        ["btn_language_ru"] = new()
    {
        { AppLanguage.English, "Русский" }, { AppLanguage.Russian, "Русский" },
        { AppLanguage.Hebrew, "Русский" }, { AppLanguage.Portuguese, "Русский" }
    },
        ["btn_language_he"] = new()
    {
        { AppLanguage.English, "עברית" }, { AppLanguage.Russian, "עברית" },
        { AppLanguage.Hebrew, "עברית" }, { AppLanguage.Portuguese, "עברית" }
    },
        ["btn_language_pt"] = new()
    {
        { AppLanguage.English, "Português" }, { AppLanguage.Russian, "Português" },
        { AppLanguage.Hebrew, "Português" }, { AppLanguage.Portuguese, "Português" }
    },

        // ── Step 2: Welcome & account check ──────────────────────────────
        ["welcome_back"] = new()
    {
        { AppLanguage.English, "👋 Welcome back, [User's Name]" },
        { AppLanguage.Russian, "👋 С возвращением, [User's Name]" },
        { AppLanguage.Hebrew, "👋 ברוך/ה שובך, [User's Name]" },
        { AppLanguage.Portuguese, "👋 Bem-vindo(a) de volta, [User's Name]" }
    },
        ["btn_create_payment_link"] = new()
    {
        { AppLanguage.English, "Create Payment Link" },
        { AppLanguage.Russian, "Create Payment Link" },
        { AppLanguage.Hebrew, "Create Payment Link" },
        { AppLanguage.Portuguese, "Criar link de pagamento" }
    },
        ["welcome_new"] = new()
    {
        { AppLanguage.English, "👋 Hi, and welcome to LamboPay\n I’ll help you create payment links so you can get paid privately and instantly in crypto." },
        { AppLanguage.Russian, "👋 Привет! Добро пожаловать в LamboPay\n Я помогу вам создавать платёжные ссылки, чтобы вы могли получать оплату приватно и мгновенно в криптовалюте." },
        { AppLanguage.Hebrew, "👋 היי, ברוך/ה הבא/ה ל-LamboPay\n אעזור לך ליצור קישורי תשלום כדי לקבל תשלום בפרטיות ובאופן מיידי בקריפטו." },
        { AppLanguage.Portuguese, "👋 Olá, bem-vindo(a) ao LamboPay\n Vou ajudar você a criar links de pagamento para receber em cripto de forma privada e instantânea." }
    },
        ["link_existing_account_btn"] = new()
    {
        { AppLanguage.English, "Link existing account" },
        { AppLanguage.Russian, "Link existing account" },
        { AppLanguage.Hebrew, "Link existing account" },
        { AppLanguage.Portuguese, "Vincular conta existente" }
    },

        // ── Step 3: Email ────────────────────────────────────────────────
        ["ask_email_with_note"] = new()
    {
        { AppLanguage.English, "📧 Please enter your email address.\n This will be your account username for the dashboard." },
        { AppLanguage.Russian, "📧 Введите свой адрес электронной почты.\n Это будет имя пользователя вашей учётной записи для панели управления." },
        { AppLanguage.Hebrew, "📧 הזן/י את כתובת האימייל שלך.\n זה יהיה שם המשתמש לחשבון שלך בלוח הבקרה." },
        { AppLanguage.Portuguese, "📧 Insira seu e-mail.\n Ele será o nome de usuário do seu painel." }
    },
        ["invalid_email"] = new()
    {
        { AppLanguage.English, "⚠️ That doesn't look like a valid email. Please try again." },
        { AppLanguage.Russian, "⚠️ Похоже, это неверный email. Попробуйте ещё раз." },
        { AppLanguage.Hebrew, "⚠️ זה לא נראה כמו אימייל תקין. נסו שוב." },
        { AppLanguage.Portuguese, "⚠️ Isso não parece um e-mail válido. Tente novamente." }
    },
        ["exist_email"] = new()
    {
        { AppLanguage.English, "⚠️ This email is already registered. Please use another one or log in to your existing account." },
        { AppLanguage.Russian, "⚠️ Этот адрес электронной почты уже зарегистрирован. Пожалуйста, используйте другой или войдите в существующую учётную запись." },
        { AppLanguage.Hebrew, "⚠️ כתובת האימייל הזו כבר רשומה. אנא השתמש/י באימייל אחר או היכנס/י לחשבון הקיים שלך." },
        { AppLanguage.Portuguese, "⚠️ Este e-mail já está registrado. Use outro ou faça login na sua conta." }
    },

        ["ask_password"] = new()
    {
        { AppLanguage.English, "🔐 Please enter your chosen password.\nThis will be your account password for the dashboard \n(Minimum 8 characters, at least one uppercase letter, one lowercase letter, one digit, and one special character)." },
        { AppLanguage.Russian, "🔐 Пожалуйста, введите выбранный пароль.\nЭто будет пароль вашей учётной записи для панели управления.\n(Минимум 8 символов, как минимум одна заглавная буква, одна строчная буква, одна цифра и один специальный символ)." },
        { AppLanguage.Hebrew, "🔐 אנא הזן/י את הסיסמה שבחרת.\nזו תהיה הסיסמה שלך לחשבון בלוח הבקרה.\n(מינימום 8 תווים, לפחות אות גדולה אחת, אות קטנה אחת, ספרה אחת ותו מיוחד אחד)." },
        { AppLanguage.Portuguese, "🔐 Insira a senha escolhida.\nEla será usada no painel.\n(Mínimo de 8 caracteres, ao menos uma maiúscula, uma minúscula, um dígito e um caractere especial)." }
    },
        ["invalid_password"] = new()
    {
        { AppLanguage.English, "⚠️ That doesn't look like a valid password. Please try again.\n(Minimum 8 characters, at least one uppercase letter, one lowercase letter, one digit, and one special character)." },
        { AppLanguage.Russian, "⚠️ Похоже, введённый пароль недействителен. Пожалуйста, попробуйте ещё раз.\n(Минимум 8 символов, как минимум одна заглавная буква, одна строчная буква, одна цифра и один специальный символ)." },
        { AppLanguage.Hebrew, "⚠️ הסיסמה שהוזנה אינה תקינה. נסה/י שוב.\n(מינימום 8 תווים, לפחות אות גדולה אחת, אות קטנה אחת, ספרה אחת ותו מיוחד אחד)." },
        { AppLanguage.Portuguese, "⚠️ Senha inválida. Tente novamente.\n(Mínimo de 8 caracteres, ao menos uma maiúscula, uma minúscula, um dígito e um caractere especial)." }
    },

        // ── Step 4: Phone ────────────────────────────────────────────────
        ["ask_phone"] = new()
    {
        { AppLanguage.English, "📱 Enter your phone number in international format (E.164)\n Example: +12025550123" },
        { AppLanguage.Russian, "📱 Введите номер телефона в международном формате (E.164)\n Пример: +12025550123" },
        { AppLanguage.Hebrew, "📱 הזן/י מספר טלפון בפורמט בינלאומי (E.164)\n דוגמה: +12025550123" },
        { AppLanguage.Portuguese, "📱 Informe seu número no formato internacional (E.164)\n Exemplo: +12025550123" }
    },
        ["invalid_phone"] = new()
    {
        { AppLanguage.English, "⚠️ Invalid format. Please try again using the format: +1234567890" },
        { AppLanguage.Russian, "⚠️ Неверный формат. Повторите, используя формат: +1234567890" },
        { AppLanguage.Hebrew, "⚠️ פורמט לא תקין. נסו שוב בפורמט: +1234567890" },
        { AppLanguage.Portuguese, "⚠️ Formato inválido. Tente novamente usando: +1234567890" }
    },

        // ── Step 5: Wallet (ERC-20, Polygon) ─────────────────────────────
        ["ask_wallet"] = new()
    {
        { AppLanguage.English, "🔗 Please enter your Polygon wallet address (ERC-20 format)\n Example: 0x1234abc..." },
        { AppLanguage.Russian, "🔗 Введите адрес вашего кошелька в сети Polygon (формат ERC-20)\n Пример: 0x1234abc..." },
        { AppLanguage.Hebrew, "🔗 הזן/י את כתובת הארנק שלך ב-Polygon (פורמט ERC-20)\n דוגמה: 0x1234abc..." },
        { AppLanguage.Portuguese, "🔗 Insira seu endereço de carteira na Polygon (formato ERC-20)\n Exemplo: 0x1234abc..." }
    },
        ["invalid_wallet"] = new()
    {
        { AppLanguage.English, "⚠️ That’s not a valid ERC-20 wallet address. Please check and try again." },
        { AppLanguage.Russian, "⚠️ Это невалидный адрес кошелька ERC-20. Проверьте и попробуйте снова." },
        { AppLanguage.Hebrew, "⚠️ זו אינה כתובת ארנק ERC-20 תקפה. בדקו ונסו שוב." },
        { AppLanguage.Portuguese, "⚠️ Esse não é um endereço ERC-20 válido. Verifique e tente novamente." }
    },

        // ── Step 6: Account created ──────────────────────────────────────
        ["account_created"] = new()
    {
        { AppLanguage.English, "🎉 Congratulations! Your account has been created successfully." },
        { AppLanguage.Russian, "🎉 Поздравляем! Ваш аккаунт успешно создан." },
        { AppLanguage.Hebrew, "🎉 מזל טוב! החשבון שלך נוצר בהצלחה." },
        { AppLanguage.Portuguese, "🎉 Parabéns! Sua conta foi criada com sucesso." }
    },
        ["account_summary"] = new()
    {
        { AppLanguage.English, "Account Summary:\n\n👤 User: [Telegram Username]\n\n📧 Email: [email]\n\n📞 Phone: [phone]\n\n👛 Wallet: [wallet address]\n\n💸 Processing Fee: 10%" },
        { AppLanguage.Russian, "Сводка аккаунта:\n\n👤 Пользователь: [Telegram Username]\n\n📧 Email: [email]\n\n📞 Телефон: [phone]\n\n👛 Кошелёк: [wallet address]\n\n💸 Комиссия за обработку: 10%" },
        { AppLanguage.Hebrew, "סיכום חשבון:\n\n👤 משתמש: [Telegram Username]\n\n📧 אימייל: [email]\n\n📞 טלפון: [phone]\n\n👛 ארנק: [wallet address]\n\n💸 עמלת עיבוד: 10%" },
        { AppLanguage.Portuguese, "Resumo da conta:\n\n👤 Usuário: [Telegram Username]\n\n📧 E-mail: [email]\n\n📞 Telefone: [phone]\n\n👛 Carteira: [wallet address]\n\n💸 Taxa de processamento: 10%" }
    },
        ["you_can_now_create"] = new()
    {
        { AppLanguage.English, "You can now create payment links and get paid in crypto." },
        { AppLanguage.Russian, "Теперь вы можете создавать платёжные ссылки и получать оплату в криптовалюте." },
        { AppLanguage.Hebrew, "כעת ניתן ליצור קישורי תשלום ולקבל תשלום בקריפטו." },
        { AppLanguage.Portuguese, "Agora você pode criar links de pagamento e receber em cripto." }
    },

        // ── Step 7: Currency selection ───────────────────────────────────
        ["select_currency"] = new()
    {
        { AppLanguage.English, "💱 Please select the currency you want your customer to pay in:" },
        { AppLanguage.Russian, "💱 Пожалуйста, выберите валюту, в которой ваш клиент будет платить:" },
        { AppLanguage.Hebrew, "💱 בחר/י את המטבע שבו הלקוח שלך ישלם:" },
        { AppLanguage.Portuguese, "💱 Selecione a moeda na qual o seu cliente pagará:" }
    },

        // ── Step 8: Amount entry ─────────────────────────────────────────
        ["enter_amount"] = new()
    {
        { AppLanguage.English, "💵 Enter the amount for the payment link\n Minimum: $50, Maximum: $15,000\n\nExample: 250.50" },
        { AppLanguage.Russian, "💵 Введите сумму для платёжной ссылки\n Минимум: $50, Максимум: $15,000\n\nПример: 250.50" },
        { AppLanguage.Hebrew, "💵 הזן/י את הסכום לקישור התשלום\n מינימום: ‎$50, מקסימום: ‎$15,000\n\nדוגמה: 250.50" },
        { AppLanguage.Portuguese, "💵 Informe o valor para o link de pagamento\n Mínimo: US$ 50, Máximo: US$ 15.000\n\nExemplo: 250.50" }
    },
        ["invalid_amount"] = new()
    {
        { AppLanguage.English, "⚠️ Please enter a number between 50–15,000 USD equivalent." },
        { AppLanguage.Russian, "⚠️ Введите число в эквиваленте USD в диапазоне 50–15 000." },
        { AppLanguage.Hebrew, "⚠️ הזן/י מספר בטווח המקביל ל-USD: ‎50–15,000." },
        { AppLanguage.Portuguese, "⚠️ Insira um número entre 50–15.000 em equivalente a USD." }
    },

        // ── Step 9: Generating links ─────────────────────────────────────
        ["links_intro"] = new()
    {
        { AppLanguage.English, "🔗 Here are your payment links:\n Different links work better depending on the customer's country and card type.\n We recommend trying them in order." },
        { AppLanguage.Russian, "🔗 Вот ваши платёжные ссылки:\n Разные ссылки работают лучше в зависимости от страны клиента и типа карты.\n Рекомендуем пробовать их по порядку." },
        { AppLanguage.Hebrew, "🔗 הנה קישורי התשלום שלך:\n קישורים שונים עובדים טוב יותר בהתאם למדינת הלקוח וסוג הכרטיס.\n אנו ממליצים לנסות לפי הסדר." },
        { AppLanguage.Portuguese, "🔗 Aqui estão seus links de pagamento:\n Links diferentes funcionam melhor dependendo do país do cliente e do tipo de cartão.\n Recomendamos tentar na ordem." }
    },
        ["amount_equivalent_line"] = new()
    {
        { AppLanguage.English, "Amount: $250.50" },
        { AppLanguage.Russian, "Сумма: $250.50" },
        { AppLanguage.Hebrew, "סכום: ‎$250.50" },
        { AppLanguage.Portuguese, "Valor: $250.50" }
    },
        ["tier1"] = new()
    {
        { AppLanguage.English, "Tier 1 – No KYC Required" },
        { AppLanguage.Russian, "Уровень 1 – KYC не требуется" },
        { AppLanguage.Hebrew, "רמה 1 – ללא צורך ב-KYC" },
        { AppLanguage.Portuguese, "Nível 1 – Sem KYC" }
    },
        ["tier2"] = new()
    {
        { AppLanguage.English, "Tier 2 – KYC Required, Higher Approval" },
        { AppLanguage.Russian, "Уровень 2 – Требуется KYC, выше шанс одобрения" },
        { AppLanguage.Hebrew, "רמה 2 – נדרש KYC, סיכויי אישור גבוהים יותר" },
        { AppLanguage.Portuguese, "Nível 2 – KYC necessário, maior aprovação" }
    },
        ["link_n"] = new()
    {
        { AppLanguage.English, "Link" },
        { AppLanguage.Russian, "Link" },
        { AppLanguage.Hebrew, "Link" },
        { AppLanguage.Portuguese, "Link" }
    },

        // ── Step 10: Instructions ────────────────────────────────────────
        ["whats_next_title"] = new()
    {
        { AppLanguage.English, "✅ What’s Next:" },
        { AppLanguage.Russian, "✅ Что дальше:" },
        { AppLanguage.Hebrew, "✅ מה הלאה:" },
        { AppLanguage.Portuguese, "✅ Próximos passos:" }
    },
        ["whats_next_copy"] = new()
    {
        { AppLanguage.English, "Copy a link and send to your customer." },
        { AppLanguage.Russian, "Скопируйте ссылку и отправьте её вашему клиенту." },
        { AppLanguage.Hebrew, "העתק/י קישור ושלח/י ללקוח." },
        { AppLanguage.Portuguese, "Copie um link e envie ao seu cliente." }
    },
        ["whats_next_try_next"] = new()
    {
        { AppLanguage.English, "If the payment fails, try the next link." },
        { AppLanguage.Russian, "Если платёж не проходит, попробуйте следующую ссылку." },
        { AppLanguage.Hebrew, "אם התשלום נכשל, נסו את הקישור הבא." },
        { AppLanguage.Portuguese, "Se o pagamento falhar, tente o próximo link." }
    },
        ["whats_next_notify"] = new()
    {
        { AppLanguage.English, "Once payment is successful, you’ll get notified here and in your dashboard." },
        { AppLanguage.Russian, "После успешного платежа вы получите уведомление здесь и в панели управления." },
        { AppLanguage.Hebrew, "לאחר שהתשלום יצליח, תקבל/י התראה כאן ובלוח הבקרה." },
        { AppLanguage.Portuguese, "Quando o pagamento for concluído, você será avisado aqui e no painel." }
    },
        ["whats_next_settlement"] = new()
    {
        { AppLanguage.English, "Funds (minus processing fee) will be sent instantly in USDT (Polygon) to your wallet." },
        { AppLanguage.Russian, "Средства (за вычетом комиссии) будут мгновенно отправлены в USDT (Polygon) на ваш кошелёк." },
        { AppLanguage.Hebrew, "הכספים (בניכוי עמלת עיבוד) יישלחו מיד ב-USDT (Polygon) לארנק שלך." },
        { AppLanguage.Portuguese, "Os fundos (menos a taxa de processamento) serão enviados instantaneamente em USDT (Polygon) para sua carteira." }
    },
        ["open_in_browser_warning"] = new()
    {
        { AppLanguage.English, "⚠️ Open the link in Chrome, Safari, or Firefox (not Telegram browser)." },
        { AppLanguage.Russian, "⚠️ Откройте ссылку в Chrome, Safari или Firefox (не в браузере Telegram)." },
        { AppLanguage.Hebrew, "⚠️ בקש/י מהלקוחות לפתוח את הקישור ב-Chrome, Safari או Firefox (לא בדפדפן של טלגרם)." },
        { AppLanguage.Portuguese, "⚠️ Peça aos clientes para abrir o link no Chrome, Safari ou Firefox (não no navegador do Telegram)." }
    },
        ["bank_transfers_title"] = new()
    {
        { AppLanguage.English, "🕒 Bank Transfers:" },
        { AppLanguage.Russian, "🕒 Банковские переводы:" },
        { AppLanguage.Hebrew, "🕒 העברות בנקאיות:" },
        { AppLanguage.Portuguese, "🕒 Transferências bancárias:" }
    },
        ["bank_sepa"] = new()
    {
        { AppLanguage.English, "SEPA: up to 24h" },
        { AppLanguage.Russian, "SEPA: до 24 часов" },
        { AppLanguage.Hebrew, "SEPA: עד 24 שעות" },
        { AppLanguage.Portuguese, "SEPA: até 24h" }
    },
        ["bank_swift"] = new()
    {
        { AppLanguage.English, "SWIFT: up to 72h" },
        { AppLanguage.Russian, "SWIFT: до 72 часов" },
        { AppLanguage.Hebrew, "SWIFT: עד 72 שעות" },
        { AppLanguage.Portuguese, "SWIFT: até 72h" }
    },
        ["btn_create_another"] = new()
    {
        { AppLanguage.English, "Create Another Link" },
        { AppLanguage.Russian, "Create Another Link" },
        { AppLanguage.Hebrew, "Create Another Link" },
        { AppLanguage.Portuguese, "Criar outro link" }
    },
        ["btn_common_issues"] = new()
    {
        { AppLanguage.English, "Common Payment Issues" },
        { AppLanguage.Russian, "Common Payment Issues" },
        { AppLanguage.Hebrew, "Common Payment Issues" },
        { AppLanguage.Portuguese, "Problemas comuns de pagamento" }
    },
        ["btn_change_language"] = new()
    {
        { AppLanguage.English, "Change Language" },
        { AppLanguage.Russian, "Change Language" },
        { AppLanguage.Hebrew, "Change Language" },
        { AppLanguage.Portuguese, "Alterar idioma" }
    },

        // ── Step 11: Common Payment Issues ───────────────────────────────
        ["issues_title"] = new()
    {
        { AppLanguage.English, "⚠️ Common Rejection Reasons" },
        { AppLanguage.Russian, "⚠️ Частые причины отклонения" },
        { AppLanguage.Hebrew, "⚠️ סיבות דחייה נפוצות" },
        { AppLanguage.Portuguese, "⚠️ Motivos comuns de rejeição" }
    },
        ["issues_identity"] = new()
    {
        { AppLanguage.English, "🔑 Identity Issues\n • Name/email doesn't match cardholder\n • Paying with someone else's card" },
        { AppLanguage.Russian, "🔑 Проблемы с идентификацией\n • Имя/email не совпадает с владельцем карты\n • Оплата чужой картой" },
        { AppLanguage.Hebrew, "🔑 בעיות זיהוי\n • שם/אימייל לא תואם לבעל הכרטיס\n • תשלום בכרטיס של מישהו אחר" },
        { AppLanguage.Portuguese, "🔑 Problemas de identidade\n • Nome/e-mail não corresponde ao titular\n • Pagamento com cartão de terceiros" }
    },
        ["issues_environment"] = new()
    {
        { AppLanguage.English, "🌐 Usage Environment\n • Telegram browser\n • VPN/proxy use\n • IP country ≠ Card country" },
        { AppLanguage.Russian, "🌐 Среда использования\n • Браузер Telegram\n • Использование VPN/прокси\n • Страна IP ≠ страна карты" },
        { AppLanguage.Hebrew, "🌐 סביבת שימוש\n • דפדפן טלגרם\n • שימוש ב-VPN/פרוקסי\n • מדינת IP ≠ מדינת הכרטיס" },
        { AppLanguage.Portuguese, "🌐 Ambiente de uso\n • Navegador do Telegram\n • Uso de VPN/proxy\n • País do IP ≠ país do cartão" }
    },
        ["issues_cards"] = new()
    {
        { AppLanguage.English, "💳 Card Issues\n • Prepaid/Anonymous cards\n • Cards not supporting 3D Secure" },
        { AppLanguage.Russian, "💳 Проблемы с картой\n • Предоплаченные/анонимные карты\n • Карты без поддержки 3D Secure" },
        { AppLanguage.Hebrew, "💳 בעיות כרטיס\n • כרטיסים נטענים/אנונימיים\n • כרטיסים שאינם תומכים ב-3D Secure" },
        { AppLanguage.Portuguese, "💳 Problemas com o cartão\n • Cartões pré-pagos/anônimos\n • Cartões sem 3D Secure" }
    },
        ["issues_other"] = new()
    {
        { AppLanguage.English, "❌ Other\n • Insufficient balance\n • Bank blocks crypto/foreign payments\n • Too many attempts\n • Incomplete KYC" },
        { AppLanguage.Russian, "❌ Другое\n • Недостаточно средств\n • Банк блокирует крипто/зарубежные платежи\n • Слишком много попыток\n • Незавершённый KYC" },
        { AppLanguage.Hebrew, "❌ אחרות\n • יתרה בלתי מספקת\n • הבנק חוסם תשלומי קריפטו/חו\"ל\n • יותר מדי ניסיונות\n • KYC לא הושלם" },
        { AppLanguage.Portuguese, "❌ Outros\n • Saldo insuficiente\n • Banco bloqueia pagamentos de cripto/exteriores\n • Tentativas demais\n • KYC incompleto" }
    },
        ["transaction_completed"] = new()
    {
        { AppLanguage.English, "✅ Your transaction has been successfully completed!\nYou can view the details in your dashboard:\n👉 [Open Dashboard]({url})" },
        { AppLanguage.Russian, "✅ Ваша транзакция успешно завершена!\nВы можете просмотреть детали в панели управления:\n👉 [Открыть панель]({url})" },
        { AppLanguage.Hebrew, "✅ העסקה שלך הושלמה בהצלחה!\nניתן לצפות בפרטים בלוח הבקרה:\n👉 [פתח לוח בקרה]({url})" },
        { AppLanguage.Portuguese, "✅ Sua transação foi concluída com sucesso!\nVocê pode ver os detalhes no painel:\n👉 [Abrir painel]({url})" }
    },
    };

    /// <summary>
    /// Достаёт строку по ключу/языку и подставляет плейсхолдеры из анонимного объекта:
    /// I18n.T("account_summary", lang, new { email = "...", phone = "..." })
    /// </summary>
    public static string T(string key, AppLanguage lang, object? args = null)
    {
        if (!Text.TryGetValue(key, out var map))
            throw new KeyNotFoundException($"I18n key '{key}' not found");

        if (!map.TryGetValue(lang, out var s))
            throw new KeyNotFoundException($"I18n key '{key}' missing lang '{lang}'");

        if (args is null) return s;

        foreach (var p in args.GetType().GetProperties())
        {
            var name = "{" + p.Name + "}";
            var value = Convert.ToString(p.GetValue(args), CultureInfo.InvariantCulture) ?? "";
            s = s.Replace(name, value);
        }
        return s;
    }
}
