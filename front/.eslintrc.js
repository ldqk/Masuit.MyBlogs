module.exports = {
  root: true,
  env: {
    node: true,
    es2020: true
  },
  extends: [
    'plugin:vue/vue3-essential',
    '@vue/standard'
  ],
  parserOptions: {
    parser: '@typescript-eslint/parser',
    ecmaVersion: 2020,
    sourceType: 'module',
    requireConfigFile: false,
    ecmaFeatures: {
      jsx: true
    }
  },
  overrides: [
    {
      files: ['*.ts', '*.tsx', '*.vue'],
      parser: 'vue-eslint-parser',
      parserOptions: {
        parser: '@typescript-eslint/parser',
        ecmaVersion: 2020,
        sourceType: 'module'
      },
      rules: {
        '@typescript-eslint/no-unused-vars': 'off',
        '@typescript-eslint/no-explicit-any': 'off',
        '@typescript-eslint/explicit-module-boundary-types': 'off',
        '@typescript-eslint/no-non-null-assertion': 'off'
      }
    }
  ],
  rules: {
    // Vue 相关规则
    'vue/multi-word-component-names': 'off',
    'vue/no-deprecated-destroyed-lifecycle': 'off',
    'vue/no-deprecated-slot-attribute': 'off',
    'vue/no-v-for-template-key-on-child': 'off',
    'vue/script-setup-uses-vars': 'error',
    
    // 基础规则
    indent: 'off',
    'n/handle-callback-err': 'off',
    'no-multiple-empty-lines': 'off',
    'no-unreachable-loop': 'off',
    'space-before-function-paren': 'off',
    'eol-last': 'off',
    semi: 'off',
    'unicode-bom': 'off',
    'no-unused-vars': 'off',
    'no-trailing-spaces': 'off',
    quotes: 'off',
    'padded-blocks': 'off',
    'comma-spacing': 'off',
    'key-spacing': 'off',
    'space-infix-ops': 'off',
    'object-curly-spacing': 'off',
    'arrow-spacing': 'off',
    'no-undef': 'off',
    'comma-dangle': 'off',
    'multiline-ternary': 'off',
    'spaced-comment': 'off',
    
    // TypeScript 相关规则
    '@typescript-eslint/space-infix-ops': 'off',
    '@typescript-eslint/no-unused-vars': 'off',
    '@typescript-eslint/no-explicit-any': 'off',
    '@typescript-eslint/explicit-module-boundary-types': 'off',
    '@typescript-eslint/no-non-null-assertion': 'off',
    
    // 导入相关规则 - 针对Vue组件中可能出现的重复导入问题
    'import/no-duplicates': 'off',
    'import/first': 'off',
    'import/no-mutable-exports': 'off',
    
    // Vue Composition API 相关规则
    'vue/no-setup-props-destructure': 'off',
    'vue/require-default-prop': 'off',
    'vue/require-prop-types': 'off',
    
    // HTML/Template 相关规则
    'vue/html-closing-bracket-newline': 'off',
    'vue/html-indent': 'off',
    'vue/max-attributes-per-line': 'off',
    'vue/singleline-html-element-content-newline': 'off',
    'vue/multiline-html-element-content-newline': 'off',
    
    // 异步/Promise 相关规则
    'prefer-promise-reject-errors': 'off',
    'node/handle-callback-err': 'off'
  }
}