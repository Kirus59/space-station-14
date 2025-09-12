cmd-party-add-member-desc = Добавляет пользователя в пати
cmd-party-add-member-help = party:adduser <id пати> <имя пользователя> [принудительно]

cmd-party-add-member-invalid-arguments-count = Неверное число аргументов!\n{ $help }
cmd-party-add-member-invalid-argument-1 = Не удалось спарсить { $arg } в качестве беззнакового целого числа (uint)
cmd-party-add-member-invalid-party-id = Не удалось найти пати с id { $id }
cmd-party-add-member-invalid-username = Не удалось найти пользователя с именем { $username }
cmd-party-add-member-invalid-argument-3 = Не удалось спарсить { $arg } в качестве boolean (True/False)
cmd-party-add-member-user-is-another-party-member = Пользователь { $user } уже является участником другого пати. Используйте параметр 'force: True' для принудительного добавления пользователя в пати
cmd-party-add-member-members-limit-reached = Целевое пати достигло лимита участников. Используйте параметр 'force: True' для игнорирования лимита участников

cmd-party-add-member-success = Пользователь { $user } успешно добавлен в пати { $partyId }!

cmd-party-add-member-hint-1 = <id пати>
cmd-party-add-member-hint-2 = <имя пользователя>
cmd-party-add-member-hint-3 = [принудительно]
cmd-party-add-member-party-hint-option = { $id } Хост: { $host }
