V = ARGV[0]
working_dir = File.expand_path(File.dirname(File.dirname(__FILE__)))

def replace_install_url(src)
  src.gsub(
    %r{(https://github.com/hadashiA/VContainer.git\?path=VContainer/Assets/VContainer#)[\d\.]+},
    %Q{\\1#{V}}
  )
end

def replace_package_json(src)
  src.gsub(
    /"version"\s*:\s*"([\d\.]+)"/,
    %Q{"version": "#{V}"})
end

def replace_docusaurus_config(src)
  src.gsub(
    /label\s*:\s*['"]v?[\d\.]+['"]/,
    %Q{'label': 'v#{V}'})
end

{
  replace_package_json: ["VContainer/Assets/VContainer/package.json"],
  replace_install_url: ["README.md", "website/docs/getting-started/installation.mdx", "website/i18n/ja/docusaurus-plugin-content-docs/current/getting-started/installation.mdx"],
  replace_docusaurus_config: ["website/docusaurus.config.ts"]
}.each do |method, relative_paths|
  relative_paths.each do |relative_path|
    path = File.join(working_dir, relative_path)
    src = File.read path
    dst = send(method, src)
    File.write path, dst
  end
end

