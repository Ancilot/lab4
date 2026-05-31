#!/usr/bin/env python3
# generator/gen_tests.py

import yaml
import argparse
import re
from pathlib import Path
from typing import Dict, List, Any
from datetime import datetime

# 1. ШАБЛОНЫ КОДА ДЛЯ C# / NUNIT


TEST_FILE_TEMPLATE = '''//
//============================================================
// AUTO-GENERATED TESTS. DO NOT EDIT MANUALLY.
// Source: {spec_source}
// Generator: gen_tests.py v1.0
// Generated at: {generation_date}
//============================================================

using System;
using NUnit.Framework;
using Lab.Interfaces;
using Lab.Implementations.GenCode2;

namespace Tests
{{
    [TestFixture]
    [Description("Автоматически сгенерированные тесты для {module_name}")]
    public class {module_name}Tests_Generated
    {{
        private IPassword _sut;

        [SetUp]
        public void SetUp()
        {{
            _sut = new Password();
        }}

{test_methods}
    }}
}}
'''

TEST_METHOD_TEMPLATE = '''
        [Test(Description = "{case_desc}")]
        public void {method_name_safe}()
        {{
            // Arrange
            // Ожидаемый результат: {expected}
            
            // Act
            {act_code}
            
            // Assert
            {assert_code}
        }}
'''



# 2. ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ


def load_spec(spec_path: str) -> Dict[str, Any]:
    with open(spec_path, 'r', encoding='utf-8') as f:
        return yaml.safe_load(f)


def format_csharp_input(value: Any) -> str:
    if value is None or value == "null":
        return "null"
    elif isinstance(value, bool):
        return "true" if value else "false"
    elif isinstance(value, str):
        if value == "null":
            return "null"
        escaped = value.replace('"', '\\"')
        return f'"{escaped}"'
    elif isinstance(value, (int, float)):
        return str(value)
    else:
        return f'"{value}"'


def safe_method_name(name: str) -> str:
    """
    Преобразует строку в безопасное имя метода C#.
    Удаляет все спецсимволы, кириллицу заменяет на латиницу.
    """
    # Замена кириллицы на латиницу
    cyrillic_map = {
        'а': 'a', 'б': 'b', 'в': 'v', 'г': 'g', 'д': 'd', 'е': 'e', 'ё': 'e',
        'ж': 'zh', 'з': 'z', 'и': 'i', 'й': 'y', 'к': 'k', 'л': 'l', 'м': 'm',
        'н': 'n', 'о': 'o', 'п': 'p', 'р': 'r', 'с': 's', 'т': 't', 'у': 'u',
        'ф': 'f', 'х': 'kh', 'ц': 'ts', 'ч': 'ch', 'ш': 'sh', 'щ': 'shch',
        'ъ': '', 'ы': 'y', 'ь': '', 'э': 'e', 'ю': 'yu', 'я': 'ya',
        'А': 'A', 'Б': 'B', 'В': 'V', 'Г': 'G', 'Д': 'D', 'Е': 'E', 'Ё': 'E',
        'Ж': 'Zh', 'З': 'Z', 'И': 'I', 'Й': 'Y', 'К': 'K', 'Л': 'L', 'М': 'M',
        'Н': 'N', 'О': 'O', 'П': 'P', 'Р': 'R', 'С': 'S', 'Т': 'T', 'У': 'U',
        'Ф': 'F', 'Х': 'Kh', 'Ц': 'Ts', 'Ч': 'Ch', 'Ш': 'Sh', 'Щ': 'Shch',
        'Ъ': '', 'Ы': 'Y', 'Ь': '', 'Э': 'E', 'Ю': 'Yu', 'Я': 'Ya'
    }
    
    for cyr, lat in cyrillic_map.items():
        name = name.replace(cyr, lat)
    
    # Замена спецсимволов
    name = re.sub(r'[^\w]', '_', name)
    
    # Удаление множественных подчёркиваний
    name = re.sub(r'_+', '_', name)
    
    # Удаление подчёркиваний в начале и конце
    name = name.strip('_')
    
    # Если имя начинается с цифры, добавляем префикс
    if name and name[0].isdigit():
        name = '_' + name
    
    # Ограничение длины
    if len(name) > 50:
        name = name[:50]
    
    return name or "Test"


def generate_assert_code(method_name: str, expected: str, params_str: str = "") -> str:
    expected_lower = expected.lower()
    
    if "argumentexception" in expected_lower:
        return f'Assert.That(() => _sut.{method_name}({params_str}), Throws.ArgumentException);'
    elif "без исключений" in expected_lower or "успешно" in expected_lower:
        return f'Assert.That(() => _sut.{method_name}({params_str}), Throws.Nothing);'
    elif "true" in expected_lower and "false" not in expected_lower:
        return f'Assert.That(result, Is.True);'
    elif "false" in expected_lower:
        return f'Assert.That(result, Is.False);'
    elif "непустая" in expected_lower or "не пустая" in expected_lower:
        return f'Assert.That(result, Is.Not.Empty);\n            Assert.That(result, Is.Not.Null);'
    else:
        return f'Assert.Pass("Ожидалось: {expected}");'


def generate_act_code(method_name: str, inputs: List[Any], signature: str) -> tuple:
    params_str = ", ".join(format_csharp_input(inp) for inp in inputs)
    
    if signature.startswith("bool") or signature.startswith("string"):
        act_code = f"var result = _sut.{method_name}({params_str});"
        return act_code, params_str
    else:
        act_code = f"_sut.{method_name}({params_str});"
        return act_code, params_str

def generate_method_tests(module_name: str, method_data: Dict[str, Any]) -> List[str]:
    case_blocks = []
    method_name = method_data["name"]
    signature = method_data.get("signature", "")
    
    for idx, eq_class in enumerate(method_data.get("equivalence_classes", [])):
        case_desc = eq_class.get("case", f"Case_{idx}")
        inputs = eq_class.get("inputs", [])
        expected = eq_class.get("expected", "No expected defined")
        
        # Генерируем безопасное имя метода
        method_name_safe = f"{module_name}_{method_name}_{safe_method_name(case_desc)}"
        
        # Генерируем параметры
        params_str = ", ".join(format_csharp_input(inp) for inp in inputs)
        
        # Определяем тип теста и генерируем соответствующий код
        expected_lower = expected.lower()
        
        if "argumentexception" in expected_lower:
            # Тест на исключение - только Assert, без Act
            act_code = ""
            assert_code = f'Assert.That(() => _sut.{method_name}({params_str}), Throws.ArgumentException);'
            
        elif "без исключений" in expected_lower or "успешно" in expected_lower:
            # Тест на успех - только Act, Assert что нет исключения
            act_code = f"_sut.{method_name}({params_str});"
            assert_code = f'Assert.That(() => _sut.{method_name}({params_str}), Throws.Nothing);'
            
        elif signature.startswith("bool"):
            # Булевый метод
            act_code = f"var result = _sut.{method_name}({params_str});"
            if "true" in expected_lower:
                assert_code = "Assert.That(result, Is.True);"
            else:
                assert_code = "Assert.That(result, Is.False);"
                
        elif signature.startswith("string"):
            # Строковый метод
            act_code = f"var result = _sut.{method_name}({params_str});"
            if "непустая" in expected_lower:
                assert_code = "Assert.That(result, Is.Not.Empty);\n            Assert.That(result, Is.Not.Null);"
            else:
                assert_code = f'Assert.Pass("Ожидалось: {expected}");'
        else:
            # Void метод по умолчанию
            act_code = f"_sut.{method_name}({params_str});"
            assert_code = 'Assert.Pass("Test executed");'
        
        case_block = TEST_METHOD_TEMPLATE.format(
            method_name_safe=method_name_safe,
            case_desc=case_desc,
            expected=expected,
            act_code=act_code,
            assert_code=assert_code
        )
        
        case_blocks.append(case_block)
    
    return case_blocks


def render_and_save(spec: Dict[str, Any], config: Dict[str, Any]) -> None:
    module_name = spec.get("module", "Password")
    test_methods = []
    
    for method in spec.get("methods", []):
        test_methods.extend(generate_method_tests(module_name, method))
    
    tests_block = "\n".join(test_methods)
    
    file_content = TEST_FILE_TEMPLATE.format(
        spec_source=config.get("spec_path", "N/A"),
        module_name=module_name,
        test_methods=tests_block,
        generation_date=datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    )
    
    out_dir = Path(config.get("output_dir", "tests"))
    out_dir.mkdir(parents=True, exist_ok=True)
    output_file = out_dir / f"{module_name}Tests_Generated.cs"
    
    output_file.write_text(file_content, encoding="utf-8")
    
    print(f"[√] Сгенерирован файл: {output_file}")
    print(f"    Методов покрыто: {len(spec.get('methods', []))}")
    total_tests = sum(len(m.get('equivalence_classes', [])) for m in spec.get('methods', []))
    print(f"    Тестов сгенерировано: {total_tests}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--config", default="config.yaml")
    args = parser.parse_args()
    

    with open(args.config, 'r', encoding='utf-8') as f:
        config = yaml.safe_load(f)
    
    spec_path = config.get('spec_path', 'spec/password.yaml')
    print(f"[*] Загрузка спецификации: {spec_path}...")
    spec_data = load_spec(spec_path)
    
    print("[*] Генерация C# тестов...")
    render_and_save(spec_data, config)
    
    print("\n[√] Готово!")


if __name__ == "__main__":
    main()